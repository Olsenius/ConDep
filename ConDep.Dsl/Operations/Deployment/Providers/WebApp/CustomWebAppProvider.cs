using System;
using ConDep.Dsl.Operations.WebDeploy.Model;

namespace ConDep.Dsl
{
    public class CustomWebAppProvider : CompositeProvider
    {
        private readonly string _webAppName;
        private readonly string _webSiteName;

        public CustomWebAppProvider(string webAppName, string webSiteName)
        {
            _webAppName = webAppName;
            _webSiteName = webSiteName;
        }

        public override bool IsValid(Notification notification)
        {
            return !string.IsNullOrWhiteSpace(_webAppName) 
                && !string.IsNullOrWhiteSpace(_webSiteName);
        }

        public override void Configure()
        {
            var command = string.Format("$webSite = Get-WebSite | where-object {{ $_.Name -eq '{0}' }}; ", _webSiteName);
            command += string.Format("if((Test-Path -path ($webSite.physicalPath + '\\{0}')) -ne $True) {{ New-Item ($webSite.physicalPath + '\\{0}') -type Directory }}; ", _webAppName);
            command += string.Format("New-WebApplication -Name \"{0}\" -Site \"{1}\" -PhysicalPath ($webSite.physicalPath + '\\{0}'); ", _webAppName, _webSiteName);
            Configure(p => p.PowerShell("Import-Module WebAdministration; " + command, o => o.WaitIntervalInSeconds(10)));
        }
    }
}