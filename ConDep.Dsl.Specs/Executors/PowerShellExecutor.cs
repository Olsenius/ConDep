﻿using System;
using System.Diagnostics;
using ConDep.Dsl.Operations.WebDeploy.Model;

namespace ConDep.Dsl.Specs.Executors
{
    public class PowerShellExecutor : ConDepOperation, IExecuteWebDeploy
    {
        private readonly string _command;

        public PowerShellExecutor(string command)
        {
            _command = command;
        }

        protected override void OnMessage(object sender, WebDeployMessageEventArgs e)
        {
            Trace.TraceInformation(e.Message);
        }

        protected override void OnErrorMessage(object sender, WebDeployMessageEventArgs e)
        {
            Trace.TraceError(e.Message);
        }

        public WebDeploymentStatus Execute()
        {
            return Setup(setup => setup.WebDeploy(s => s
                                                           .WithConfiguration(c => c.DoNotAutoDeployAgent())
                                                           .From.LocalHost()
                                                           .UsingProvider(p => p
                                                                                   .PowerShell(_command))
                                                           .To.LocalHost()
                                      ));
        }

        public WebDeploymentStatus ExecuteFromPackage()
        {
            throw new NotImplementedException();
        }
    }

}