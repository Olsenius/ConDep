using System;
using ConDep.Dsl.Operations.WebDeploy.Model;
using Microsoft.Web.Deployment;

namespace ConDep.Dsl
{
	public class RunCmdProvider : Provider
	{
	    private readonly bool _continueOnError;
	    private Exception _untrappedExitCodeException;
        private const string NAME = "runCommand";

		public RunCmdProvider(string command, bool continueOnError)
		{
		    _continueOnError = continueOnError;
		    DestinationPath = command;
		}

	    public override DeploymentObject GetWebDeploySourceObject(DeploymentBaseOptions sourceBaseOptions)
		{
			return DeploymentManager.CreateObject(Name, "", sourceBaseOptions);
		}

		public override string Name
		{
			get { return NAME; }
		}

		public override DeploymentProviderOptions GetWebDeployDestinationObject()
		{
			var destProviderOptions = new DeploymentProviderOptions(Name) { Path = DestinationPath };
            //DeploymentProviderSetting dontUseCmdExe;
            //if (destProviderOptions.ProviderSettings.TryGetValue("dontUseCommandExe", out dontUseCmdExe))
            //{
            //    dontUseCmdExe.Value = true;
            //}
		    return destProviderOptions;
		}

		public override bool IsValid(Notification notification)
		{
			return !string.IsNullOrWhiteSpace(DestinationPath) ||
                string.IsNullOrWhiteSpace(SourcePath);
		}

        public override WebDeploymentStatus Sync(WebDeployOptions webDeployOptions, WebDeploymentStatus deploymentStatus)
        {
            try
            {
                webDeployOptions.DestBaseOptions.Trace += CheckForUntrappedRunCommandExitCodes;
                base.Sync(webDeployOptions, deploymentStatus);
            }
            catch
            {
                if(!_continueOnError)
                {
                    throw;
                }
            }
            finally
            {
                webDeployOptions.DestBaseOptions.Trace -= CheckForUntrappedRunCommandExitCodes;

                if (_untrappedExitCodeException != null && !_continueOnError)
                {
                    throw _untrappedExitCodeException;
                }
            }
            return deploymentStatus;
        }

        void CheckForUntrappedRunCommandExitCodes(object sender, DeploymentTraceEventArgs e)
        {
            //Terrible hack to trap exit codes that the WebDeploy runCommand ignores!
            if (e.Message.Contains("exited with code "))
            {
                if (!e.Message.Contains("exited with code '0x0'"))
                {
                    _untrappedExitCodeException = new Exception(e.Message, _untrappedExitCodeException);
                }
            }
        }

	}
}