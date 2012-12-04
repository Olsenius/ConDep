﻿namespace ConDep.Dsl.Operations.Application.Deployment.NServiceBus
{
    public class NServiceBusOptions
    {
        private readonly NServiceBusOperation _nservicebusProvider;

        public NServiceBusOptions(NServiceBusOperation nservicebusProvider)
        {
            _nservicebusProvider = nservicebusProvider;
        }
        
    	public NServiceBusOptions ServiceInstaller(string nServiceBusInstallerPath)
    	{
    		_nservicebusProvider.ServiceInstallerName = nServiceBusInstallerPath;
			return this;
    	}

    	public NServiceBusOptions UserName(string username)
    	{
    		_nservicebusProvider.ServiceUserName = username;
    		return this;
    	}

    	public NServiceBusOptions Password(string password)
    	{
    		_nservicebusProvider.ServicePassword = password;
			return this;
    	}


    	public NServiceBusOptions ServiceGroup(string group)
    	{
    		_nservicebusProvider.ServiceGroup = group;
			return this;
    	}

        /// <summary>
        /// Specifies which profile NServiceBus should run under
        /// </summary>
        public NServiceBusOptions Profile(string profile)
        {
            _nservicebusProvider.Profile = profile;
            return this;
        }

        /// <summary>
        /// Interval in seconds with no failures after which the failure count is reset to 0
        /// </summary>
        /// <param name="interval">Interval in seconds</param>
        /// <returns></returns>
        public NServiceBusOptions ServiceFailureResetInterval(int interval)
        {
            _nservicebusProvider.ServiceFailureResetInterval = interval;
            return this;
        }

        /// <summary>
        /// Delay in millisecond before the service gets restarted after failure
        /// </summary>
        /// <param name="delay">Delay in milliseconds</param>
        /// <returns></returns>
        public NServiceBusOptions ServiceRestartDelay(int delay)
        {
            _nservicebusProvider.ServiceRestartDelay = delay;
            return this;
        }

        public NServiceBusOptions IgnoreFailureOnServiceStartStop(bool value)
        {
            _nservicebusProvider.IgnoreFailureOnServiceStartStop = value;
            return this;
        }
    }
}