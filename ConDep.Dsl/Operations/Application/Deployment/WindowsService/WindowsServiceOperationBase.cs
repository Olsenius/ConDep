namespace ConDep.Dsl.Operations.Application.Deployment.WindowsService
{
    public abstract class WindowsServiceOperationBase : RemoteCompositeOperation
    {
        protected readonly string _serviceName;
        protected readonly string _sourceDir;
        protected readonly string _destDir;
        protected readonly string _relativeExePath;
        protected readonly string _displayName;
        protected readonly WindowsServiceOptions.WindowsServiceOptionValues _values;

        protected WindowsServiceOperationBase(string serviceName, string displayName, string sourceDir, string destDir, string relativeExePath, WindowsServiceOptions.WindowsServiceOptionValues values)
        {
            _serviceName = serviceName;
            _sourceDir = sourceDir;
            _destDir = destDir;
            _relativeExePath = relativeExePath;
            _displayName = displayName;
            _values = values;
        }

        public override void Configure(IOfferRemoteComposition server)
        {
            ConfigureRemoveService(server);
            ConfigureDeployment(server);
            ConfigureInstallService(server);
            ConfigureServiceFailure(server);
            ConfigureServiceConfig(server);
            ConfigureServiceStart(server);
        }

        protected void ConfigureDeployment(IOfferRemoteComposition server)
        {
            server.Deploy.Directory(_sourceDir, _destDir);
        }

        protected void ConfigureServiceStart(IOfferRemoteComposition server)
        {
            if (!_values.DoNotStart)
            {
                var start = string.Format("Start-ConDepWinService '{0}' 0 {1}", _serviceName,
                                          "$" + _values.IgnoreFailureOnServiceStartStop);
                server.ExecuteRemote.PowerShell(start,
                                                o =>
                                                o.WaitIntervalInSeconds(60).ContinueOnError(
                                                    _values.IgnoreFailureOnServiceStartStop));
            }
        }

        protected void ConfigureServiceConfig(IOfferRemoteComposition server)
        {
            var serviceConfigCommand = _values.GetServiceConfigCommand(_serviceName);
            if (!string.IsNullOrWhiteSpace(serviceConfigCommand)) server.ExecuteRemote.DosCommand(serviceConfigCommand);
        }

        protected void ConfigureServiceFailure(IOfferRemoteComposition server)
        {
            var serviceFailureCommand = _values.GetServiceFailureCommand(_serviceName);
            if (!string.IsNullOrWhiteSpace(serviceFailureCommand)) server.ExecuteRemote.DosCommand(serviceFailureCommand);
        }

        protected void ConfigureRemoveService(IOfferRemoteComposition server)
        {
            var remove = string.Format("Remove-ConDepWinService '{0}' 0 {1}", _serviceName,
                                       "$" + _values.IgnoreFailureOnServiceStartStop);
            server.ExecuteRemote.PowerShell(remove,
                                            o =>
                                            o.WaitIntervalInSeconds(60).ContinueOnError(_values.IgnoreFailureOnServiceStartStop));
        }

        protected abstract void ConfigureInstallService(IOfferRemoteComposition server);
    }
}