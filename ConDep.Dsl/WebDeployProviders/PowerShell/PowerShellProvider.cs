using ConDep.Dsl.Core;

namespace ConDep.Dsl
{
    public class PowerShellProvider : WebDeployCompositeProviderBase
    {
        public PowerShellProvider(string command)
        {
            DestinationPath = command;
        }

        public bool ContinueOnError { get; set; }

        public override void Configure(DeploymentServer server)
        {
            Configure<ProvideForInfrastructure>(server, AddChildProvider, po => po.RunCmd(string.Format(@"powershell.exe -InputFormat none -Command ""& {{ $ErrorActionPreference='stop'; {0}; exit $LASTEXITCODE }}""", DestinationPath), this.ContinueOnError, o => o.WaitIntervalInSeconds(this.WaitInterval)));
        }

        public override bool IsValid(Notification notification)
        {
            return string.IsNullOrWhiteSpace(SourcePath) && !string.IsNullOrWhiteSpace(DestinationPath);
        }
    }
}