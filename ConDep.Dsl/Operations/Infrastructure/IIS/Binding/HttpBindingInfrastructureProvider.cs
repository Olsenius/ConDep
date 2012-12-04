﻿using System;
using ConDep.Dsl.Builders;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Operations.Infrastructure.IIS.Binding
{
    public class HttpBindingInfrastructureProvider : RemoteCompositeOperation
    {
        private readonly string _webSiteName;
        private readonly int _port;

        public HttpBindingInfrastructureProvider(string webSiteName, int port)
        {
            if(string.IsNullOrWhiteSpace(webSiteName)) throw new ArgumentNullException("webSiteName");

            _webSiteName = webSiteName;
            _port = port;
        }

        public HttpBindingInfrastructureProvider(string webSiteName, int port, Action<IisBindingInfrastructureOptions> bindingOptions)
        {
            _webSiteName = webSiteName;
            _port = port;
            throw new NotImplementedException();
        }

        public string HostHeader { get; set; }

        public string Ip { get; set; }

        public override void Configure(IOfferRemoteOperations server)
        {
            //Todo: get bindingtype
            var psCommand = CreateBinding(_webSiteName, Ip, HostHeader, _port.ToString(), BindingType.http);
            server.ExecuteRemote.PowerShell("Import-Module WebAdministration; " + psCommand, o => o.WaitIntervalInSeconds(2).RetryAttempts(20));
        }

        public override bool IsValid(Notification notification)
        {
            //todo: add validation
            return true;
        }

        private static string CreateBinding(string webSiteName, string ip, string hostHeader, string port, BindingType bindingType)
        {
            var ipAddress = string.IsNullOrWhiteSpace(ip)
                                ? ""
                                : "-IPAddress \"" + ip + "\"";

            var hostHeader2 = string.IsNullOrWhiteSpace(hostHeader)
                                 ? ""
                                 : "-HostHeader \"" + hostHeader + "\"";

            var commandParams = string.Format("-Name \"{0}\" -Protocol \"{1}\" -Port {2} {3} {4}; ",
                              webSiteName, bindingType, port, ipAddress, hostHeader2);

            var newBinding = "New-WebBinding " + commandParams;
            var getBinding = string.Format("$binding = Get-WebBinding " + commandParams + "; if($binding -eq $null) {{ {0} }}", newBinding);
            return getBinding;
        }
    }
}