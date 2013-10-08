﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Remote.Node.Model;
using Newtonsoft.Json.Linq;

namespace ConDep.Dsl.Remote.Node
{
    public class Api
    {
        private HttpClient _client;

        public Api(string url, string userName, string password)
        {
            var messageHandler = new HttpClientHandler { Credentials = new NetworkCredential(userName, password) };
            _client = new HttpClient(messageHandler) { BaseAddress = new Uri(url) };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public SyncResult SyncDir(string srcPath, string dstPath)
        {
            try
            {
                var urlTemplate = DiscoverUrl("http://www.con-dep.net/rels/sync/dir_template");
                var url = string.Format(urlTemplate, dstPath);
                return SyncDirByUrl(srcPath, url);
            }
            catch (AggregateException aggrEx)
            {
                throw aggrEx.Flatten();
            }
        }

        public SyncResult SyncFile(string srcPath, string dstPath)
        {
            try
            {
                var url = DiscoverUrl("http://www.con-dep.net/rels/sync/file_template");
                var syncResponse = _client.GetAsync(string.Format(url, dstPath)).Result;

                if (syncResponse.IsSuccessStatusCode)
                {
                    var nodeFile = syncResponse.Content.ReadAsAsync<SyncDirFile>().Result;
                    return CopyFile(srcPath, _client, nodeFile);
                }
                return null;
            }
            catch (AggregateException aggrEx)
            {
                throw aggrEx.Flatten();
            }
        }

        public SyncResult SyncWebApp(string webSiteName, string webAppName, string srcPath, string dstPath = null)
        {
            try
            {
                var url = DiscoverUrl("http://www.con-dep.net/rels/iis_template");
                var url2 = url.Replace("{website}", webSiteName).Replace("{webapp}", webAppName);

                var syncResponse = _client.GetAsync(url2).Result;

                if (syncResponse.IsSuccessStatusCode)
                {
                    var webAppInfo = syncResponse.Content.ReadAsAsync<WebAppInfo>().Result;
                    if (!string.IsNullOrWhiteSpace(dstPath) && webAppInfo.Exist && webAppInfo.PhysicalPath != dstPath)
                    {
                        throw new ArgumentException(string.Format("Web app {0} already exist and physical path differs from path provided.", webAppName));
                    }

                    var path = string.IsNullOrWhiteSpace(dstPath) ? webAppInfo.PhysicalPath : dstPath;
                    foreach (var link in webAppInfo.Links)
                    {
                        switch (link.Rel)
                        {
                            case "http://www.con-dep.net/rels/iis/web_app_template":
                                CreateWebApp(link, path);
                                break;
                            case "http://www.con-dep.net/rels/sync/dir_template":
                                return SyncDirByUrl(srcPath, string.Format(link.Href, path));
                            case "http://www.con-dep.net/rels/sync/directory":
                                return SyncDirByUrl(srcPath, link.Href);
                        }
                    }
                }
                return null;
            }
            catch (AggregateException aggrEx)
            {
                throw aggrEx.Flatten();
            }
        }

        private SyncResult SyncDirByUrl(string srcPath, string url)
        {
            try
            {
                var syncResponse = _client.GetAsync(url).Result;

                if (syncResponse.IsSuccessStatusCode)
                {
                    var nodeDir = syncResponse.Content.ReadAsAsync<SyncDirDirectory>().Result;
                    return CopyFiles(srcPath, _client, nodeDir);
                }
                return null;
            }
            catch (AggregateException aggrEx)
            {
                throw aggrEx.Flatten();
            }
        }

        private string DiscoverUrl(string rel)
        {
            try
            {
                Logger.Verbose(string.Format("Finding url for [{0}]", rel));

                var availableApiResourcesResponse = _client.GetAsync("api").Result;
                if (availableApiResourcesResponse == null)
                    throw new Exception("Response was empty");

                Logger.Verbose(string.Format("Status code for response is [{0}]", availableApiResourcesResponse.StatusCode));

                var availableApiResourcesContent = availableApiResourcesResponse.Content.ReadAsAsync<JToken>().Result;
                if (availableApiResourcesContent == null)
                {
                    var actualResponse = availableApiResourcesResponse.Content.ReadAsStringAsync().Result;

                    throw new Exception("Content of response was empty. Actual response was: " + actualResponse);
                }

                var url = (from link in availableApiResourcesContent
                           where link.Value<string>("rel") == rel
                           select link.Value<string>("href")).SingleOrDefault();
                return url;
            }
            catch (AggregateException aggrEx)
            {
                throw aggrEx.Flatten();
            }
        }

        private void CreateWebApp(Link link, string path)
        {
            var message = new HttpRequestMessage { Method = link.HttpMethod, RequestUri = new Uri(string.Format(link.Href, path)) };

            var syncResponse = _client.SendAsync(message).Result;

        }

        private SyncResult CopyFile(string srcFile, HttpClient client, SyncDirFile nodeFile)
        {
            var message = new HttpRequestMessage();

            var clientFile = new FileInfo(srcFile);
            if (nodeFile.EqualTo(clientFile, clientFile.Directory.FullName))
            {
                return new SyncResult();
            }

            var fileStream = new FileStream(clientFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var content = new StreamContent(fileStream);

            var link = nodeFile.Links.GetByRel("http://www.con-dep.net/rels/sync/file_sync_template");
            var url = string.Format(link.Href, clientFile.LastWriteTimeUtc.ToFileTime(), clientFile.Attributes.ToString());

            message.Method = link.HttpMethod;
            message.Content = content;
            message.RequestUri = new Uri(url);

            var result = client.SendAsync(message).ContinueWith(task =>
            {
                if (task.Result.IsSuccessStatusCode)
                {
                    var syncResult = task.Result.Content.ReadAsAsync<SyncResult>().Result;
                    return syncResult;
                }
                return null;
            });
            result.Wait();
            return result.Result;
        }

        private SyncResult CopyFiles(string srcRoot, HttpClient client, SyncDirDirectory nodeDir)
        {
            var message = new HttpRequestMessage();
            var content = new MultipartSyncDirContent();

            var clientDir = new DirectoryInfo(srcRoot);
            var diffs = nodeDir.Diff(clientDir);

            var files = diffs.MissingAndChangedPaths;

            foreach (var file in files)
            {
                if (file.IsDirectory) continue;

                var fileInfo = new FileInfo(file.Path);
                var fileStream = new FileStream(fileInfo.FullName, FileMode.Open);
                content.Add(new StreamContent(fileStream), "file", file.RelativePath, fileInfo.Attributes, fileInfo.LastWriteTimeUtc);
            }

            var postProcContent = new SyncPostProcContent
            {
                DeletedFiles = diffs.DeletedFiles,
                DeletedDirectories = diffs.DeletedDirectories,
                ChangedDirectories = diffs.ChangedDirectories
            };
            content.Add(new ObjectContent<SyncPostProcContent>(postProcContent, new JsonMediaTypeFormatter()));

            var link = nodeDir.Links.GetByRel("http://www.con-dep.net/rels/sync/directory");

            message.Method = link.HttpMethod;
            message.Content = content;
            message.RequestUri = new Uri(link.Href);

            var result = client.SendAsync(message).ContinueWith(task =>
            {
                if (task.Result.IsSuccessStatusCode)
                {
                    var syncResult = task.Result.Content.ReadAsAsync<SyncResult>().Result;
                    return syncResult;
                }
                return null;
            });
            result.Wait();
            return result.Result;
        }

        public bool Validate()
        {
            try
            {
                var availableApiResourcesResponse = _client.GetAsync("api").Result;
                if (availableApiResourcesResponse == null)
                {
                    Logger.Verbose(string.Format("No response from ConDep Node when calling {0}/{1}", _client.BaseAddress, "api"));
                    return false;
                }

                var availableApiResourcesContent = availableApiResourcesResponse.Content.ReadAsAsync<JToken>().Result;
                if (availableApiResourcesContent == null)
                {
                    Logger.Verbose(string.Format("No content retreived from ConDep Node when reading content from {0}/{1}", _client.BaseAddress, "api"));
                    return false;
                }

                if (availableApiResourcesContent.Count() == 0)
                {
                    Logger.Verbose(string.Format("No content retreived from ConDep Node when reading content from {0}/{1}", _client.BaseAddress, "api"));
                    return false;
                }
                Logger.Verbose("Successfully validated ConDep Node.");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}