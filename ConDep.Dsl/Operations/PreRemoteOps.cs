﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ConDep.Dsl.Config;
using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations.Application.Deployment.CopyFile;
using ConDep.Dsl.Operations.Application.Execution.PowerShell;
using ConDep.Dsl.PSScripts;
using ConDep.Dsl.Resources;
using ConDep.Dsl.SemanticModel;
using ConDep.Dsl.SemanticModel.Sequence;

namespace ConDep.Dsl.Operations
{
    internal class PreRemoteOps
    {
        private readonly ServerConfig _server;
        private readonly PreOpsSequence _sequence;
        private readonly ConDepSettings _settings;

        public PreRemoteOps(ServerConfig server, PreOpsSequence sequence, ConDepSettings settings)
        {
            _server = server;
            _sequence = sequence;
            _settings = settings;
        }

        public void Configure()
        {
            ConfigureCopyResource(Assembly.GetExecutingAssembly(), PowerShellResources.PowerShellScriptResources);

            if(_settings.Options.Assembly != null)
            {
                var assemblyResources = _settings.Options.Assembly.GetManifestResourceNames();
                ConfigureCopyResource(_settings.Options.Assembly, assemblyResources);
            }
        }

        private void ConfigureCopyResource(Assembly assembly, IEnumerable<string> resources)
        {
            if (resources == null || assembly == null) return;
            
            foreach (var path in resources.Select(resource => ExtractPowerShellFileFromResource(assembly, resource)).Where(path => !string.IsNullOrWhiteSpace(path)))
            {
                ConfigureCopyFileOperation(path);
            }
        }

        private string ExtractPowerShellFileFromResource(Assembly assembly, string resource)
        {
            var regex = new Regex(@".+\.(.+\.(ps1|psm1))");
            var match = regex.Match(resource);
            if (match.Success)
            {
                var resourceName = match.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    var resourceNamespace = resource.Replace("." + resourceName, "");
                    return ConDepResourceFiles.GetFilePath(assembly, resourceNamespace, resourceName, true);
                }
            }
            return null;
        }

        private void ConfigureCopyFileOperation(string path)
        {
            var copyFileOperation = new CopyFileOperation(path, string.Format(@"%windir%\temp\ConDep\{0}\PSScripts\ConDep\{1}", ConDepGlobals.ExecId, Path.GetFileName(path)));
            _sequence.Add(copyFileOperation, true);
        }

        public IReportStatus Execute(IReportStatus status)
        {
            TempInstallConDepNode(status);
            return status;
        }

        private void TempInstallConDepNode(IReportStatus status)
        {
            Logger.LogSectionStart("Deploying ConDep Node");
            try
            {
                var listenUrl = "http://{0}:80/ConDepNode/";
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ConDepNode.exe");
                var byteArray = File.ReadAllBytes(path);
                var psCopyFileOp = new ConDepNodePublisher(byteArray, string.Format(@"${{env:windir}}\temp\ConDep\{0}\{1}", ConDepGlobals.ExecId, Path.GetFileName(path)), string.Format(listenUrl, "localhost"));
                psCopyFileOp.Execute(_server);
                Logger.Info(string.Format("ConDep Node successfully deployed to {0}", _server.Name));
                Logger.Info(string.Format("Node listening on {0}", string.Format(listenUrl, _server.Name)));
            }
            finally
            {
                Logger.LogSectionEnd("Deploying ConDep Node");
            }
        }
    }
}