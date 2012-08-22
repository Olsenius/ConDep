using System;
using System.Diagnostics;

namespace ConDep.Dsl.Core
{
	public class WebDeployMessageEventArgs : EventArgs
	{
		public string Message { get; set; }
		public TraceLevel Level { get; set; }
	}
}