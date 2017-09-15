using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ucsb.Sa.Enterprise.ClientExtensions;

namespace Ucsb.Sa.Enterprise.MvcExtensions
{
	public class HttpCallLogContainer
	{

		/// <summary>
		/// Gets or sets the service log entry. This should always be set.
		/// </summary>
		public HttpCall Call { get; set; }

		/// <summary>
		/// Gets or sets the error log entry. This might be null if no exception occurred.
		/// </summary>
		public HttpError Error { get; set; }


	}
}
