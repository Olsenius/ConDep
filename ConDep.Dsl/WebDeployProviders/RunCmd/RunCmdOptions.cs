namespace ConDep.Dsl
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
			_provider.WaitInterval = waitInterval;
			return this;
		}
	}
}