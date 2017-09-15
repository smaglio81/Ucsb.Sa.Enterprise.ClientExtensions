using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	public class DelegatingHandlerDefinition
	{
		/// <summary>
		/// Name of the class within the assembly.
		/// </summary>
		public string ClassName { get; set; }

		/// <summary>
		/// The name of the assembly to find the class in. Try to
		/// avoid adding the version, culture or publicKeyToken. If
		/// just the name is given, then it's easier to match the
		/// assembly even if the version information changes.
		/// </summary>
		public string AssemblyName { get; set; }
	}
}
