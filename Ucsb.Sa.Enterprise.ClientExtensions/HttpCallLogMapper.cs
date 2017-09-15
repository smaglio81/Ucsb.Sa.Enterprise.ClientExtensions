using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	public static class HttpCallLogMapper
	{
		/// <summary>
		/// Create the Call object which can be saved to the tracing tables. This will
		/// not save. Use SaveCall to create the object and save.
		/// </summary>
		/// <param name="request">Http Request</param>
		/// <param name="response">Http Response</param>
		/// <param name="requestDateTime">Request Date time </param>
		/// <param name="responseDateTime">Response Datetime</param>
		/// <param name="getRequestBody">A function to get the body. Depending on the direction on the request, this is different.</param>
		/// <param name="direction">In for incoming/recieved request (a website will recieved). Out for call made to other websites.</param>
		public static HttpCall MapHttpCall(
			HttpRequestMessage request,
			HttpResponseMessage response,
			DateTime requestDateTime,
			DateTime responseDateTime,
			Func<string> getRequestBody
		)
		{
			HttpCall call = new HttpCall();

			// request info
			call.RequestBody = getRequestBody();
			call.Method = request.Method.Method;
			call.Server = Environment.MachineName;
			call.RequestDate = requestDateTime;

			//	if the requestUri has the schema in it (http://) then only the request uri should
			//	be recorded. The HttpClient object will ignore the BaseAddress if the full url path
			//	is given in the requestUri.
			call.Uri = request.RequestUri.AbsoluteUri;

			// request header 
			string headers = string.Empty;
			int counter = 0;

			foreach (var header in request.Headers)
			{
				headers += header.Key + " = " + header.Value.ElementAt(counter) + Environment.NewLine;

			}
			call.RequestHeader = headers;

			call.ResponseDate = responseDateTime;   // this always has to be set or EF blows up because .NET DateTime Range
													// starts at 1/1/0001, and that date is father back than SQL's DateTime
													// can handle. SQL's DateTime2 can handle it, but we didn't use that.
													// Error Message: The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value
			if (response != null)
			{
				// response info 
				call.ResponseBody = response.Content.ReadAsStringAsync().Result;
				call.StatusCode = (int)response.StatusCode;

				// create header info from header collection 
				var responseheaders = string.Empty;
				foreach (var header in response.Headers)
				{
					responseheaders += header.Key + " = " + header.Value + Environment.NewLine;
				}
				// Header info from request header 
				call.ResponseHeader = responseheaders;
			}

			// time diff between call 
			call.TimeDiff = responseDateTime - requestDateTime;
			// direction of request 
			call.Direction = RequestDirection.Out;

			return call;
		}

		/// <summary>
		/// On Error call this even handler 
		/// </summary>
		/// <param name="request">HTTP Request</param>
		/// <param name="requestDateTime">Date/Time of the request</param>
		/// <param name="exception">Any exceptions that have occured.</param>
		public static List<HttpError> MapHttpErrors(
			HttpRequestMessage request,
			DateTime requestDateTime,
			Exception exception
		) {
			var errors = new List<HttpError>();

			var uri = request.RequestUri.AbsoluteUri;

			var currentException = exception;
			while (currentException != null)
			{
				var error = new HttpError();
				error.Uri = uri;
				error.RequestDate = requestDateTime;
				error.Message = currentException.Message;
				error.Source = currentException.Source;
				error.StackTrace = currentException.StackTrace;
				error.TargetSite = currentException.TargetSite.Name;
				error.Type = currentException.GetType().FullName;

				errors.Add(error);
				currentException = currentException.InnerException;
			}



			return errors;
		}
	}
}
