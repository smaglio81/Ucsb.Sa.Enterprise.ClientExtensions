using System;
using System.ComponentModel.DataAnnotations;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// A log entry of a request that was processed.
	/// </summary>
	public class HttpCall : HttpCallDto
    {

		public HttpCall() {}

		public HttpCall(HttpCall copyFrom)
			: base(copyFrom)
		{
			RequestHeader = copyFrom.RequestHeader;
			RequestCookie = copyFrom.RequestCookie;
			RequestBody = copyFrom.RequestBody;
			ResponseCookie = copyFrom.ResponseCookie;
			ResponseHeader = copyFrom.ResponseHeader;
			ResponseBody = copyFrom.ResponseBody;
		}

		/// <summary>
		/// Gets or sets the request headers. The headers are delimited by Environment.NewLine.
		/// </summary>
        public string RequestHeader { get; set; }

		/// <summary>
		/// Gets or sets the request cookies. The cookies are delimited by Environment.NewLine.
		/// </summary>
        public string RequestCookie { get; set; }

		/// <summary>
		/// Gets or sets the request body. Useful for POST and PUT requests.
		/// </summary>
        public string RequestBody { get; set; }

		/// <summary>
		/// Gets or sets the response cookies that were set during the processing of the request.
		/// The cookies will be delimited by Environment.NewLine.
		/// </summary>
        public string ResponseCookie { get; set; }

		/// <summary>
		/// Gets or sets the response headers. The headers will be delimited by Environment.NewLine.
		/// </summary>
        public string ResponseHeader { get; set; }

		/// <summary>
		/// Gets or sets the response body.
		/// </summary>
        public string ResponseBody { get; set; }

    }
}
