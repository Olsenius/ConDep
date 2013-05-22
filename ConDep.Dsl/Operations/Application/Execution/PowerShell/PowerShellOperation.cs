using System.IO;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Operations.Application.Execution.PowerShell
{
    public class PowerShellOperation : RemoteCompositeOperation
    {
        private readonly FileInfo _scriptFile;
        private readonly PowerShellOptions.PowerShellOptionValues _values;
        private readonly string _command;
        private int _waitInterval = 30;

        public PowerShellOperation(string command)
        {
            _command = command;
        }

        public PowerShellOperation(FileInfo scriptFile, PowerShellOptions.PowerShellOptionValues values = null)
        {
            _scriptFile = scriptFile;
            _values = values;
        }

        public bool ContinueOnError { get; set; }
        public int WaitIntervalInSeconds { get { return _waitInterval; } set { _waitInterval = value; } }
        public int RetryAttempts { get; set; }

        public bool RequireRemoteLib { get; set; }

        public override void Configure(IOfferRemoteComposition server)
        {
            string libImport = "";

            //var script = AddExitCodeHandlingToScript(DestinationPath);
            //var filePath = CreateScriptFile(script);
            //var destFilePath = CopyScriptToDestination(server, filePath);
            //ExecuteScriptOnDestination(server, destFilePath);

            if(RequireRemoteLib)
            {
                server.Deploy.File(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ConDep.Remote.dll"), @"%temp%\ConDep.Remote.dll");
                libImport = "Add-Type -Path \"" + @"%temp%\ConDep.Remote.dll" + "\";";
            }
            //elseif($Error.Count -gt 0) {{ Write-Error $Error[0]; exit 1; }} 
            server.ExecuteRemote.DosCommand(string.Format(@"powershell.exe -noprofile -InputFormat none -Command ""& {{ set-executionpolicy remotesigned -force; $ErrorActionPreference='stop'; Import-Module $env:windir\temp\ConDep\{0}\PSScripts\ConDep; {1}{2}; if(!$?) {{ exit 1; }} else {{ exit $LASTEXITCODE; }} }}""", ConDepGlobals.ExecId, libImport, _command), o => o.ContinueOnError(ContinueOnError));
        }

        //private string AddExitCodeHandlingToScript(string script)
        //{
        //    var builder = new StringBuilder(script);
        //    builder.Insert(0, "$ErrorActionPreference='stop';" + Environment.NewLine);
        //    builder.Append(Environment.NewLine + "exit $LASTEXITCODE");
        //    return builder.ToString();
        //}

        //private void ExecuteScriptOnDestination(ServerConfig server, string destFilePath)
        //{
        //    Configure<ProvideForInfrastructure>(server, po => po.RunCmd(string.Format(@"powershell.exe -InputFormat none -File '{0}'", destFilePath), ContinueOnError, o => o.WaitIntervalInSeconds(WaitInterval)));
        //}

        //private string CopyScriptToDestination(ServerConfig server, string filePath)
        //{
        //    var destFilePath = @"%temp%\" + Path.GetFileName(filePath);
        //    Configure<ProvideForDeployment>(server, c => c.CopyFile(filePath, destFilePath));
        //    return destFilePath;
        //}

        //private string CreateScriptFile(string script)
        //{
        //    var fileName = new Guid().ToString() + ".condep";
        //    var filePath = Path.Combine(Path.GetTempPath(), fileName);
        //    File.WriteAllText(filePath, script);
        //    return filePath;
        //}

        public override string Name
        {
            get { return "PowerShell"; }
        }

        public override bool IsValid(Notification notification)
        {
            return true;
            //var remoteLibExist = true;
            //if(RequireRemoteLib)
            //{
            //    remoteLibExist = File.Exists(Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ConDep.Remote.dll"));
            //}
            //return string.IsNullOrWhiteSpace(_command) && !string.IsNullOrWhiteSpace(DestinationPath) && remoteLibExist;
        }
    }
}