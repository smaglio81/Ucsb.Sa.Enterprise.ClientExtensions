using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	public class HttpErrorDto
	{

		public HttpErrorDto() {}

		public HttpErrorDto(HttpErrorDto copyFrom)
		{
			ErrorId = copyFrom.ErrorId;
			CallId = copyFrom.CallId;
			RequestDate = copyFrom.RequestDate;
			Type = copyFrom.Type;
			Message = copyFrom.Message;
			Source = copyFrom.Source;
			TargetSite = copyFrom.TargetSite;
			Uri = copyFrom.Uri;
		}

		public int ErrorId { get; set; }

		public int CallId { get; set; }

		/// <summary>
		/// Gets or sets the date and time the request was recieved.
		/// </summary>
		public DateTime RequestDate { get; set; }

		[StringLength(500)]
		public string Uri { get; set; }

		[StringLength(200)]
		public string Type { get; set; }

		[StringLength(500)]
		public string Message { get; set; }

		[StringLength(200)]
		public string Source { get; set; }

		[StringLength(100)]
		public string TargetSite { get; set; }

	}
}
