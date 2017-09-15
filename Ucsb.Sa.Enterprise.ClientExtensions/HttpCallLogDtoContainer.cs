using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ucsb.Sa.Enterprise.ClientExtensions;

namespace Ucsb.Sa.Enterprise.MvcExtensions
{
	public class HttpCallLogDtoContainer
	{

		/// <summary>
		/// Gets or sets the service log entry. This should always be set.
		/// </summary>
		public HttpCallDto Call { get; set; }

		/// <summary>
		/// Gets or sets the error log entry. This might be null if no exception occurred.
		/// </summary>
		public HttpErrorDto Error { get; set; }

	}
}
