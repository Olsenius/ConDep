using ConDep.Dsl.Logging;
using ConDep.Dsl.Operations.Application.Local;

namespace ConDep.Dsl.SemanticModel.WebDeploy
{
	public class WebDeployOperation : LocalOperation
	{
		private readonly WebDeployServerDefinition _webDeployServerDefinition;

		public WebDeployOperation(WebDeployServerDefinition webDeployServerDefinition)
		{
			_webDeployServerDefinition = webDeployServerDefinition;
		}

        public override IReportStatus Execute(IReportStatus status)
		{
		    _webDeployServerDefinition.TraceLevel = Logger.TraceLevel;

            if(BeforeExecute != null)
            {
                BeforeExecute(_webDeployServerDefinition.WebDeployDestination.ComputerName, status);
                if(status.HasErrors) return status;
            }

            var syncStatus = _webDeployServerDefinition.Sync(status);

            if (status.HasErrors) return status;

            if (AfterExecute != null)
            {
                AfterExecute(_webDeployServerDefinition.WebDeployDestination.ComputerName, status);
            }
		    return syncStatus;
		}

		public override bool IsValid(Notification notification)
		{
			return _webDeployServerDefinition.IsValid(notification);
		}
	}
}