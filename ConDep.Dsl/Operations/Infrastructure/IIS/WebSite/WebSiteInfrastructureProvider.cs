using System;
using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Operations.Infrastructure.IIS.WebSite
{
    public class WebSiteInfrastructureProvider : RemoteCompositeOperation, IRequireCustomConfiguration
    {
        private readonly string _webSiteName;
        private readonly int _id;
        private readonly IisWebSiteOptions _options;
        private readonly IList<IisBinding> _bindings = new List<IisBinding>();

        public WebSiteInfrastructureProvider(string webSiteName, int id)
        {
            _webSiteName = webSiteName;
            _id = id;
            _options = new IisWebSiteOptions();
        }

        public WebSiteInfrastructureProvider(string webSiteName, int id, IisWebSiteOptions options)
        {
            _webSiteName = webSiteName;
            _id = id;
            _options = options;
        }

        public string WebSiteName { get { return _webSiteName; } }

        public IList<IisBinding> Bindings { get { return _bindings; } }

        public string AppPoolName { get; set; }

        public override bool IsValid(Notification notification)
        {
            return !string.IsNullOrWhiteSpace(_webSiteName);
        }

        public override void Configure(IOfferRemoteComposition server)
        {
            //var webSiteConfig = server.WebSites.SingleOrDefault(x => x.Name == WebSiteName);

            string psCommand = GetRemoveExistingWebSiteCommand(_id);
            ////psCommand += GetCreateAppPoolCommand();
            psCommand += GetCreateWebSiteDirCommand(PhysicalDirectory);
            psCommand += GetCreateWebSiteCommand(_webSiteName, AppPoolName);
            //psCommand += GetCreateBindings(_webSiteName, Bindings, webSiteConfig.Bindings);
            //psCommand += GetCertificateCommand();

            server.ExecuteRemote.PowerShell("Import-Module WebAdministration; " + psCommand, o => o.WaitIntervalInSeconds(2).RetryAttempts(20));
        }

        protected string PhysicalDirectory { get; set; }

        private string GetCreateWebSiteDirCommand(string webSiteDir)
        {
            return string.IsNullOrWhiteSpace(webSiteDir) ? "" : string.Format("if((Test-Path -path {0}) -ne $True) {{ New-Item {0} -type Directory }}; ", webSiteDir);
        }

        private string GetRemoveExistingWebSiteCommand(int webSiteId)
        {
            return string.Format("get-website | where-object {{ $_.ID -match '{0}' }} | Remove-Website; ", webSiteId);
        }

        private string GetCertificateCommand()
        {
            string command = "";
            foreach(var binding in Bindings)
            {
                if(binding.BindingType == BindingType.https)
                {
                    var bindingIp = string.IsNullOrWhiteSpace(binding.Ip) ? "0.0.0.0" : binding.Ip;
                    command += string.Format("Set-Location IIS:\\SslBindings; Remove-Item {0}!{1} -ErrorAction SilentlyContinue; ", bindingIp, binding.Port);
                    command += string.Format("$webSiteCert = Get-ChildItem cert:\\LocalMachine\\MY | Where-Object {{$_.Subject -match 'CN=*{0}*'}} | Select-Object -First 1; ", binding.CertificateCommonName);
                    command += string.Format("if($webSiteCert -eq $null) {{ throw 'No Certificate with CN=''*{0}*'' found.' }}; ", binding.CertificateCommonName);
                    command += string.Format("$webSiteCert | New-Item {0}!{1}; ", bindingIp, binding.Port);
                }
            }
            return command;
        }

        private string GetCreateBindings(string webSiteName, IList<IisBinding> bindings, IList<WebSiteBindingConfig> serverWebSiteBindings)
        {
            string bindingString = "";

            if(bindings.Count > 0)
            {
                for (int index = 1; index < Bindings.Count; index++)
                {
                    var binding = Bindings[index];
                    bindingString += CreateBinding(webSiteName, binding.Ip, binding.HostHeader, binding.Port.ToString(), binding.BindingType);
                }

                foreach(var binding in serverWebSiteBindings)
                {
                    var bindingType = binding.BindingType.ToLower() == "https" ? BindingType.https : BindingType.http;
                    bindingString += CreateBinding(webSiteName, binding.Ip, binding.HostHeader, binding.Port.ToString(), bindingType);
                }
            }
            else
            {
                for (int index = 1; index < serverWebSiteBindings.Count; index++)
                {
                    var binding = serverWebSiteBindings[index];
                    var bindingType = binding.BindingType.ToLower() == "https" ? BindingType.https : BindingType.http;
                    bindingString += CreateBinding(webSiteName, binding.Ip, binding.HostHeader, binding.Port.ToString(), bindingType);
                }
            }
            return bindingString;
        }

        private string CreateBinding(string webSiteName, string ip, string hostHeader, string port, BindingType bindingType)
        {
                var ipAddress = string.IsNullOrWhiteSpace(ip)
                                    ? ""
                                    : "-IPAddress \"" + ip + "\"";

                var hostHeader2 = string.IsNullOrWhiteSpace(hostHeader)
                                     ? ""
                                     : "-HostHeader \"" + hostHeader + "\"";

                return string.Format("New-WebBinding -Name \"{0}\" -Protocol \"{1}\" -Port {2} {3} {4} -force; ",
                                  webSiteName, bindingType, port, ipAddress, hostHeader2);

        }

        private string GetCreateWebSiteCommand(string webSiteName, string appPoolName)
        {
            string bindingString = "";
            //if(Bindings.Count > 0)
            //{
            //    var binding = Bindings[0];
            //    bindingString = GetFirstWebSiteBinding(binding.Port.ToString(), binding.BindingType, binding.Ip, binding.HostHeader);
            //} 
            //else if(serverWebSiteBindings != null && serverWebSiteBindings.Count() > 0)
            //{
            //    var binding = serverWebSiteBindings[0];
            //    var bindingType = binding.BindingType.ToLower() == "https" ? BindingType.https : BindingType.http;
            //    bindingString = GetFirstWebSiteBinding(binding.Port, bindingType, binding.Ip, binding.HostHeader);
            //}

            var appPool = !string.IsNullOrWhiteSpace(appPoolName) ? string.Format(" -ApplicationPool \"{0}\" ", appPoolName) : "";
            var physicalPath = string.IsNullOrWhiteSpace(PhysicalDirectory) ? "" : string.Format("-PhysicalPath \"{0}\" ", PhysicalDirectory);
            var port = "";// _options.PortNumber > 0 ? "-Port " + _options.PortNumber + " " : "";
            return string.Format("$newWebSite = New-Website -Name \"{0}\" -Id {1} {2}{3}{4}{5}-force; if($newWebSite.State -eq 'Stopped') {{ throw 'Failed to start web site.' }} else {{ $newWebSite; }} ", webSiteName, _id, physicalPath, bindingString, appPool, port);
        }

        private string GetFirstWebSiteBinding(string port, BindingType bindingType, string ip, string hostHeader)
        {
            string bindingString = "";
            bindingString += "-Port " + port + " ";
            bindingString += bindingType == BindingType.https ? "-Ssl " : "";
            bindingString += !string.IsNullOrWhiteSpace(ip) ? "-IPAddress \"" + ip +"\" " : "";
            bindingString += !string.IsNullOrWhiteSpace(hostHeader) ? "-HostHeader \"" + hostHeader + "\" " : "";
            //bindingString += _applicationPool.Name != null ? string.Format("-ApplicationPool '{0}' ", _applicationPool.Name) : "";
            return bindingString;
        }
    }
}