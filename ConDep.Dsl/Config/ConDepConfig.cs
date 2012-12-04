﻿using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    public class ConDepConfig
    {
        public string EnvironmentName { get; set; }
        public LoadBalancerConfig LoadBalancer { get; set; }
        public IList<ServerConfig> Servers { get; set; }
        public DeploymentUserConfig DeploymentUser { get; set; }
        public IList<CustomProviderConfig> CustomProviderConfig { get; set; }
    }
}