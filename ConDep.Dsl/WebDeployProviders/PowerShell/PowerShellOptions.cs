﻿namespace ConDep.Dsl.WebDeployProviders.PowerShell
{
    public class PowerShellOptions
    {
        private readonly PowerShellProvider _powerShellProvider;

        public PowerShellOptions(PowerShellProvider powerShellProvider)
        {
            _powerShellProvider = powerShellProvider;
        }

        public PowerShellOptions ContinueOnError(bool value)
        {
            _powerShellProvider.ContinueOnError = value;
            return this;
        }

        public PowerShellOptions WaitIntervalInSeconds(int seconds)
        {
            _powerShellProvider.WaitInterval = seconds;
            return this;
        }

        public PowerShellOptions RetryAttempts(int attempts)
        {
            _powerShellProvider.RetryAttempts = attempts;
            return this;
        }
    }
}