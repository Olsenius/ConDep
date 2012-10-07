using System;
using System.Diagnostics;
using ConDep.Dsl.WebDeploy;

namespace ConDep.Dsl
{
    internal class ConDepContextOperationPlaceHolder : ConDepOperationBase
    {
        private string _contextName;

        public ConDepContextOperationPlaceHolder(string contextName)
        {
            ContextName = contextName;
        }

        public string ContextName
        {
            get { return _contextName; }
            set { _contextName = value; }
        }

        public override WebDeploymentStatus Execute(WebDeploymentStatus webDeploymentStatus)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(Notification notification)
        {
            return true;
        }
    }
}