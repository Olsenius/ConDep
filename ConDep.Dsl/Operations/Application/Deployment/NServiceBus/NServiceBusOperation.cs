using System.IO;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Resources;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Operations.Application.Deployment.NServiceBus
{
    public class NServiceBusOperation : RemoteCompositeOperation, IRequireCustomConfiguration
    {
        internal const string SERVICE_CONTROLLER_EXE = @"C:\WINDOWS\system32\sc.exe";
        private string _serviceInstallerName = "NServiceBus.Host.exe";
        private string _sourcePath;
        private string _destPath;

        public NServiceBusOperation(string path, string destDir, string serviceName)
        {
            _sourcePath = Path.GetFullPath(path);
            ServiceName = serviceName;
            _destPath = destDir;
        }

        public string ServicePassword { get; set; }
        public string ServiceUserName { get; set; }

        internal string ServiceName { get; set; }
        internal string ServiceGroup { get; set; }
        internal string Profile { get; set; }
        internal int? ServiceFailureResetInterval { get; set; }
        internal int? ServiceRestartDelay { get; set; }
        internal bool IgnoreFailureOnServiceStartStop { get; set; }

        public string ServiceInstallerName
        {
            get { return _serviceInstallerName; }
            set { _serviceInstallerName = value; }
        }

        public override void Configure(IOfferRemoteComposition server)
        {
            CopyPowerShellScriptsToTarget(server.Deploy);

            var install = string.Format("{0} /install /serviceName:\"{1}\" /displayName:\"{1}\" {2}", Path.Combine(_destPath, ServiceInstallerName), ServiceName, Profile);

            var serviceFailureCommand = "";
            var serviceConfigCommand = "";

            if (HasServiceFailureOptions)
            {
                var serviceResetOption = ServiceFailureResetInterval.HasValue ? "reset= " + ServiceFailureResetInterval.Value : "";
                var serviceRestartDelayOption = ServiceRestartDelay.HasValue ? "actions= restart/" + ServiceRestartDelay.Value : "";

                serviceFailureCommand = string.Format("{0} failure \"{1}\" {2} {3}", SERVICE_CONTROLLER_EXE, ServiceName, serviceResetOption, serviceRestartDelayOption);
            }

            if (HasServiceConfigOptions)
            {
                var userNameOption = !string.IsNullOrWhiteSpace(ServiceUserName) ? "obj= \"" + ServiceUserName + "\"" : "";
                var passwordOption = !string.IsNullOrWhiteSpace(ServicePassword) ? "password= \"" + ServicePassword + "\"" : "";
                var groupOption = !string.IsNullOrWhiteSpace(ServiceGroup) ? "group= \"" + ServiceGroup + "\"" : "";

                serviceConfigCommand = string.Format("{0} config \"{1}\" {2} {3} {4}", SERVICE_CONTROLLER_EXE, ServiceName, userNameOption, passwordOption, groupOption);
            }

            var remove = string.Format(". $env:temp\\NServiceBus.ps1; remove-nsbservice {0}", ServiceName);
            server.ExecuteRemote.PowerShell(remove, o => o.ContinueOnError(IgnoreFailureOnServiceStartStop).WaitIntervalInSeconds(60));
            server.Deploy.Directory(_sourcePath, _destPath);

            //Allow continue on error??
            server.ExecuteRemote.DosCommand(install, opt => opt.WaitIntervalInSeconds(60));
            if (!string.IsNullOrWhiteSpace(serviceFailureCommand)) server.ExecuteRemote.DosCommand(serviceFailureCommand);
            if (!string.IsNullOrWhiteSpace(serviceConfigCommand)) server.ExecuteRemote.DosCommand(serviceConfigCommand);

            var start = string.Format(". $env:temp\\NServiceBus.ps1; start-nsbservice {0}", ServiceName);
            server.ExecuteRemote.PowerShell(start, o => o.WaitIntervalInSeconds(60).ContinueOnError(IgnoreFailureOnServiceStartStop));
        }

        public override string Name
        {
            get { return "NServiceBus"; }
        }

        public override bool IsValid(Notification notification)
        {
            throw new System.NotImplementedException();
        }

        private void CopyPowerShellScriptsToTarget(IOfferRemoteDeployment deploy)
        {
            var filePath = ConDepResourceFiles.GetFilePath(GetType().Namespace, "NServiceBus.ps1");
            deploy.File(filePath, @"%temp%\NServiceBus.ps1");
        }

        private bool HasServiceConfigOptions
        {
            get { return !string.IsNullOrWhiteSpace(ServiceUserName) || !string.IsNullOrWhiteSpace(ServicePassword) || !string.IsNullOrWhiteSpace(ServiceGroup); }
        }

        private bool HasServiceFailureOptions
        {
            get { return ServiceFailureResetInterval.HasValue || ServiceRestartDelay.HasValue; }
        }

    }
}