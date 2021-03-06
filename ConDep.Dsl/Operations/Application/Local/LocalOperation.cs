using System;
using ConDep.Dsl.Config;
using ConDep.Dsl.SemanticModel;

namespace ConDep.Dsl.Operations.Application.Local
{
    public abstract class LocalOperation : IValidate
	{
        public Action<string, IReportStatus> BeforeExecute;
        public abstract IReportStatus Execute(IReportStatus status, ConDepConfig config, ConDepOptions options);
        public Action<string, IReportStatus> AfterExecute;
        public abstract bool IsValid(Notification notification);
	}
}