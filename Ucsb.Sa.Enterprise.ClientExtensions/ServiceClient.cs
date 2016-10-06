using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// A baseline ServiceClient object to help simplify construction with Ninject
	/// </summary>
	/// <typeparam name="T">An interface that implements a service</typeparam>
	public class ServiceClient<T>
	{
		public ServiceClient() {} //throw new Exception("Please don't use!"); }

		public ServiceClient(IKernel kernel)
		{
			this.Kernel = kernel;
		}

		public IKernel Kernel { get; set; }

		/// <summary>
		/// Returns a new instance of an interface that returns a service. When
		/// constructed properly, this should return a proxy when a web service will
		/// be called. And a concrete service object when the service will be used
		/// inline.
		/// </summary>
		public T Service { get { return Kernel.Get<T>(); } }
	}
}
