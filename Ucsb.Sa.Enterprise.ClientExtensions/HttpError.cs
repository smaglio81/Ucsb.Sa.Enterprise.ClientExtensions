using System;
using System.ComponentModel.DataAnnotations;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	public class HttpError : HttpErrorDto
    {

		public HttpError() {}

		public HttpError(HttpError copyFrom)
			: base(copyFrom)
		{
			StackTrace = copyFrom.StackTrace;
		}

        public string StackTrace { get; set; }

    }
}
