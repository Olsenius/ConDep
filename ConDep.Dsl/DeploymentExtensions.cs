﻿using System;

namespace ConDep.Dsl.Core
{
    public static class DeploymentExtensions
    {
        public static void Iis(this ProvideForDeployment deploymentOptions, Action<ProvideForDeploymentIis> iisOptions)
        {
            iisOptions(new ProvideForDeploymentIis());
        }
    }
}