using System.Collections.Generic;
using System.Linq;

namespace ConDep.Dsl.SemanticModel.Sequence
{
    public class ExecutionSequenceManager
    {
        private readonly List<LocalSequence> _sequence = new List<LocalSequence>();

        public void Add(LocalSequence localOperation)
        {
            _sequence.Add(localOperation);
        }

        public LocalSequence NewLocalSequence()
        {
            var sequence = new LocalSequence();
            _sequence.Add(sequence);
            return sequence;
        }

        public IReportStatus Execute(IReportStatus status)
        {
            foreach (var localSequence in _sequence)
            {
                localSequence.Execute(status);
                if (status.HasErrors)
                    return status;
            }
            return status;
        }

        public bool IsValid(Notification notification)
        {
            return _sequence.All(x => x.IsValid(notification));
        }
    }
}