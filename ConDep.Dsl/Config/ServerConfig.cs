using System.Collections.Generic;

namespace ConDep.Dsl.Config
{
    public class ServerConfig
    {
        private DeploymentUserConfig _deploymentUserRemote;
        private string _agentUrl;

        public string Name { get; set; }
        public IList<WebSiteConfig> WebSites { get; set; }
        public DeploymentUserConfig DeploymentUserRemote 
        { 
            get { return _deploymentUserRemote ?? (_deploymentUserRemote = new DeploymentUserConfig()); }
            set { _deploymentUserRemote = value; }
        }

        public string WebDeployAgentUrl { 
            get
            {
                if(string.IsNullOrEmpty(_agentUrl))
                {
                    _agentUrl = Name;
                }
                return _agentUrl;
            }
            set
            {
                _agentUrl = value;
            }
        }
    }
}