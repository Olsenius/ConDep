namespace ConDep.Dsl.Operations.Application.Execution.RunCmd
{
	public class RunCmdOptions
	{
		private readonly RunCmdProvider _provider;

		public RunCmdOptions(RunCmdProvider provider)
		{
			_provider = provider;
		}

		public RunCmdOptions WaitIntervalInSeconds(int waitInterval)
		{
			_provider.WaitIntervalInSeconds = waitInterval;
			return this;
		}

	    public RunCmdOptions RetryAttempts(int retryAttempts)
	    {
            _provider.RetryAttempts = retryAttempts;
            return this;
        }
	}
}