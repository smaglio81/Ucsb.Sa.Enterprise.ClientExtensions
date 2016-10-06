using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// A limited response object for single object responses.
	/// </summary>
	public class SaSingleResponse
	{
		/// <summary>
		/// This is a message about the status of the response. In general this will be "OK". But, it can
		/// be customized for error details.
		/// </summary>
		public string Message { get; set; }
		
		/// <summary>
		/// The object to return.
		/// </summary>
		public object Result { get; set; }
	}
}
