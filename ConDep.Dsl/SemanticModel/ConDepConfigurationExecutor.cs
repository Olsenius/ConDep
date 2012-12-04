using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.SemanticModel.Sequence;
using TinyIoC;

namespace ConDep.Dsl.SemanticModel
{
    public class ConDepConfigurationExecutor
    {
        public static void ExecuteFromAssembly(Assembly assembly, ConDepConfig envSettings, ConDepOptions options, IReportStatus status)
        {
            new ConDepConfigurationExecutor().Execute(assembly, envSettings, options, status);
        }

        private void Execute(Assembly assembly, ConDepConfig envConfig, ConDepOptions options, IReportStatus status)
        {
            if (assembly == null) { throw new ArgumentException("assembly"); }
            if (envConfig == null) { throw new ArgumentException("envSettings"); }
            if (options == null) { throw new ArgumentException("options"); }
            if (status == null) { throw new ArgumentException("status"); }

            var applications = new List<ApplicationArtifact>();
            if(options.HasContext())
            {
                var type = assembly.GetTypes().Where(t => typeof(ApplicationArtifact).IsAssignableFrom(t) && t.Name == options.Context).Single();
                if (type == null)
                {
                    throw new ConDepConfigurationTypeNotFoundException(string.Format("A class inheriting from [{0}] must be present in assembly [{1}] for ConDep to work.", typeof(ApplicationArtifact).FullName, assembly.FullName));
                }

                var application = assembly.CreateInstance(type.FullName) as ApplicationArtifact;
                if (application == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] in assembly [{1}] is not found.", type.FullName, assembly.FullName));
                applications.Add(application);
            } 
            else
            {
                var types = assembly.GetTypes().Where(t => typeof(ApplicationArtifact).IsAssignableFrom(t));
                foreach(var type in types)
                {
                    var application = assembly.CreateInstance(type.FullName) as ApplicationArtifact;
                    if (application == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] in assembly [{1}] is not found.", type.FullName, assembly.FullName));
                    applications.Add(application);
                }
            }

            IoCBootstrapper.Bootstrap(envConfig);
            var sequence = TinyIoCContainer.Current.Resolve<IManageExecutionSequence>();

            foreach(var application in applications)
            {
                var local = new LocalOperationsBuilder(sequence);
                application.Configure(local, envConfig);
            }

            var notification = new Notification();
            if (!sequence.IsValid(notification))
            {
                notification.Throw();
            }

            sequence.Execute(status);
        }
    }
}