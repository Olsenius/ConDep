﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Web.Deployment;

namespace ConDep.Dsl.WebDeploy
{
	public abstract class WebDeployProviderBase : IProvide, IValidate
	{
	    private readonly List<IProvideConditions> _conditions = new List<IProvideConditions>();

        public string SourcePath { get; set; }
		public virtual string DestinationPath { get; set; }
		public abstract string Name { get; }
		public int WaitInterval { get; set; }

	    public abstract DeploymentProviderOptions GetWebDeployDestinationObject();
		public abstract DeploymentObject GetWebDeploySourceObject(DeploymentBaseOptions sourceBaseOptions);
        public virtual IList<DeploymentRule> GetReplaceRules() { return new List<DeploymentRule>(); }

		public abstract bool IsValid(Notification notification);

	    public void AddCondition(IProvideConditions condition)
	    {
	        _conditions.Add(condition);
	    }

	    public virtual WebDeploymentStatus Sync(WebDeployOptions webDeployOptions, WebDeploymentStatus deploymentStatus)
        {
            if (HasConditions())
            {
                if (_conditions.Any(condition => !condition.HasExpectedOutcome(webDeployOptions))) return deploymentStatus;
            }

            var defaultWaitInterval = webDeployOptions.DestBaseOptions.RetryInterval;

            if (WaitInterval > 0)
            {
                webDeployOptions.DestBaseOptions.RetryInterval = WaitInterval * 1000;
            }

            DeploymentChangeSummary summery;
            using (var sourceDepObject = webDeployOptions.FromPackage ? GetPackageSourceObject(webDeployOptions) : GetWebDeploySourceObject(webDeployOptions.SourceBaseOptions))
            {
                var destProviderOptions = GetWebDeployDestinationObject();

                foreach (var rule in GetReplaceRules())
                {
                    webDeployOptions.SyncOptions.Rules.Add(rule);
                }

                //if(webDeployOptions.FromPackage)
                //{
                //    summery = sourceDepObject.SyncTo(DeploymentWellKnownProvider.Auto, "", webDeployOptions.DestBaseOptions, webDeployOptions.SyncOptions);
                //}
                //else
                //{

                //Backup(webDeployOptions);
                summery = sourceDepObject.SyncTo(destProviderOptions, webDeployOptions.DestBaseOptions, webDeployOptions.SyncOptions);
                //}
            }

            deploymentStatus.AddSummery(summery);

            webDeployOptions.DestBaseOptions.RetryInterval = defaultWaitInterval;

            if (summery.Errors > 0)
            {
                throw new WebDeployProviderException("The provider reported " + summery.Errors + " during deployment.");
            }
            return deploymentStatus;
        }

	    private bool HasConditions()
	    {
	        return _conditions.Count > 0;
	    }

	    private void Backup(WebDeployOptions webDeployOptions)
	    {
	        bool existOnDestination = true;
            try
            {
                var dest = DeploymentManager.CreateObject(Name, DestinationPath, webDeployOptions.DestBaseOptions);
                var children = dest.GetChildren();
                if (children.Count() == 0)
                    existOnDestination = false;
            }
            catch
            {
                existOnDestination = false;
            }

            if(existOnDestination)
            {
                var backupSource = DeploymentManager.CreateObject(Name, DestinationPath, webDeployOptions.DestBaseOptions);
                backupSource.SyncTo(DeploymentWellKnownProvider.Package, @"C:\Temp\PackageBackup.zip",
                                    webDeployOptions.DestBaseOptions, webDeployOptions.SyncOptions);
            }
	    }

	    private static DeploymentObject GetPackageSourceObject(WebDeployOptions webDeployOptions)
	    {
            return DeploymentManager.CreateObject(DeploymentWellKnownProvider.Package, webDeployOptions.PackagePath, webDeployOptions.SourceBaseOptions);
	    }
	}
}