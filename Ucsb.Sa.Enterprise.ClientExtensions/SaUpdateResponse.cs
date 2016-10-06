using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// A CQRS style update resposne.
	/// </summary>
	public class SaUpdateResponse
	{
		/// <summary>
		/// This is a message about the status of the response. In general this will be "OK". But, it can
		/// be customized for error details.
		/// </summary>
		public string Message { get; set; }
		
		/// <summary>
		/// The unique id of the record that was created.
		/// </summary>
		public string Id { get; set; }
		
		/// <summary>
		/// The base uri for the endpoint that hosts the newly created object.
		/// </summary>
		public string BaseUri { get; set; }
		
		/// <summary>
		/// The instance uri for the newly created object.
		/// </summary>
		public string InstanceUri { get; set; }
	}
}
