﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{

	/// <summary>
	/// The calls that are logged can be either Incoming or Outgoing.
	/// This "enumartion" describes the direction.
	/// </summary>
	public static class RequestDirection
	{
		/// <summary>
		/// In means Incoming, or recieved by the application.
		/// </summary>
		public const string In = "In";

		/// <summary>
		/// Out is for Outgoing, or sent to a client.
		/// </summary>
		public const string Out = "Out";
	}

	/// <summary>
	/// A smaller DTO object for use with searching.
	/// </summary>
	public class HttpCallDto
	{

		public HttpCallDto() {}

		public HttpCallDto(HttpCallDto copyFrom)
		{
			CallId = copyFrom.CallId;
			Server = copyFrom.Server;
			IP = copyFrom.IP;
			Method = copyFrom.Method;
			Uri = copyFrom.Uri;
			RequestDate = copyFrom.RequestDate;
			StatusCode = copyFrom.StatusCode;
			ResponseDate = copyFrom.ResponseDate;
			TimeDiff = copyFrom.TimeDiff;
			Metadata = copyFrom.Metadata;
		}

		/// <summary>
		/// Gets or sets the MVC service log entry identifier. Generated by the database.
		/// </summary>
		public int CallId { get; set; }

		/// <summary>
		/// Gets or sets the name of the server.
		/// </summary>
		[StringLength(10)]
		public string Server { get; set; }

		/// <summary>
		/// Gets or sets the client ip.
		/// </summary>
		[StringLength(15)]
		public string IP { get; set; }

		/// <summary>
		/// Gets or sets the request URI.
		/// </summary>
		[StringLength(500)]
		public string Uri { get; set; }

		/// <summary>
		/// Gets or sets the date and time the request was recieved.
		/// </summary>
		public DateTime RequestDate { get; set; }

		/// <summary>
		/// Gets or sets the date and time the response was sent (end of processing).
		/// </summary>
		public DateTime ResponseDate { get; set; }

		/// <summary>
		/// Gets or sets the time difference between the request being recieved and the response
		/// being sent (the processing time). This is tracked using a StopWatch object, which
		/// is very precise.
		/// </summary>
		public TimeSpan TimeDiff { get; set; }

		/// <summary>
		/// Gets or sets the request direction. In means Incoming, or recieved by the application.
		/// Out is for Outgoing, or sent to a client.
		/// </summary>
		[StringLength(3)]
		public string Direction { get; set; }

		/// <summary>
		/// Gets or sets the response status code. The response HTTP status code. Hopefully
		/// always "200 OK".
		/// </summary>
		public int StatusCode { get; set; }

		/// <summary>
		/// Gets or sets the request method.
		/// </summary>
		[StringLength(10)]
		public string Method { get; set; }

		/// <summary>
		/// Gets or sets the metadata. Controlled by the developer to add custom data for entries
		/// that will be useful for their applications.
		/// </summary>
		[StringLength(500)]
		public string Metadata { get; set; }

	}
}