using System;
using System.Collections.Generic;
using System.Linq;

namespace ConDep.Dsl.SemanticModel
{
	public class Notification
	{
		private readonly List<SemanticValidationError> _validationErrors = new List<SemanticValidationError>();

		public bool HasErrors
		{
			get { return _validationErrors.Count > 0; }
		}

		public void AddError(SemanticValidationError error)
		{
			_validationErrors.Add(error);
		}

		public bool HasErrorOfType(ValidationErrorType errorType)
		{
			return _validationErrors.Any(e => e.ErrorType == errorType);
		}

	    public void Throw()
	    {
	        if(_validationErrors.Count > 0)
            {
                throw _validationErrors.Aggregate<SemanticValidationError, Exception>(null, (current, error) => new ConDepInvalidSetupException(error.Message, current));
            }
	        throw new ConDepInvalidSetupException("Validation failed for unknown reason");
	    }
	}
}