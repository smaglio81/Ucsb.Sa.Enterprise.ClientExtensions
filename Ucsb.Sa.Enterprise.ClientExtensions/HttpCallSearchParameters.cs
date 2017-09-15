using System;
using System.Collections.Generic;

namespace Ucsb.Sa.Enterprise.MvcExtensions
{
	/// <summary>
	/// Search parameters for the MvcExteions tables.
	/// </summary>
	public class HttpCallSearchParameters
	{

		public HttpCallSearchParameters()
		{
			CallIds = new List<int>();
			ServerNames = new List<string>();
			IPs = new List<string>();
			StatusCodes = new List<int>();
			Methods = new List<string>();
		}

		/// <summary>
		/// Gets the list of Call Ids to filter on. If empty, then the Call Ids
		/// are not filtered on.
		/// </summary>
		public List<int> CallIds { get; internal set; }

		/// <summary>
		/// Gets the list of Server Names to filter on. If empty, then the Server Names
		/// are not filtered on.
		/// </summary>
		public List<string> ServerNames { get; internal set; }

		/// <summary>
		/// Gets the list of Client IPs to filter on. If empty, then the Client IPs
		/// are not filtered on.
		/// </summary>
		public List<string> IPs { get; set; }

		/// <summary>
		/// Gets or sets the URI pattern to search on. This pattern is in the
		/// format of a T-SQL LIKE statement.
		/// </summary>
		public string UriPattern { get; set; }

		/// <summary>
		/// Gets or sets the start date/time. The result set will be inclusive
		/// of this time.
		/// </summary>
		public DateTime? Start { get; set; }

		/// <summary>
		/// Gets or sets the end date/time. The result set will be inclusive
		/// of this time.
		/// </summary>
		public DateTime? End { get; set; }

		/// <summary>
		/// The earliest point in time to start retrieving results from. If null,
		/// the search will start from the earliest record in the database.
		/// </summary>
		public TimeSpan? MinTimeDiff { get; set; }

		/// <summary>
		/// The latest point in time to stop retrieving results from. If null,
		/// the search will end at the most recent result in the table.
		/// </summary>
		public TimeSpan? MaxTimeDiff { get; set; }
		
		/// <summary>
		/// Gets or sets the Direction filter. If this value is null, then
		/// no Direction filter will be applied. The Direction values are
		/// found in <see cref="RequestDirection" />.
		/// </summary>
		public string Direction { get; set; }

		/// <summary>
		/// Gets or sets the list of HTTP Status Codes to filter on. No
		/// sub-status codes are used. If the list is empty, then no
		/// filter will be applied.
		/// </summary>
		public List<int> StatusCodes { get; internal set; }

		/// <summary>
		/// Gets or sets a list of HTTP method types that can be filtered on.
		/// If empty, no filter will be applied to the HTTP method type. Method
		/// types are GET, POST, etc.
		/// </summary>
		public List<string> Methods { get; internal set; }

		/// <summary>
		/// Gets or sets the Metadata Filter. This filter assumes you already know
		/// the format of the data in each calls metadata. The supplied function
		/// will need to parse the metadata and determine if a match occurs or not.
		/// If MetadataFilter is null, then no filter is applied.
		/// </summary>
		public Func<string,bool> MetadataFilter { get; set; }

		/// <summary>
		/// Filter the results to only calls which have associated errors.
		/// </summary>
		public bool ErrorsOnly { get; set; }

		/// <summary>
		/// Normally the objects returned from the searches are DTOs, which
		/// don't contain the properties which can have long string values.
		/// Setting this parameters ensures the full objects are returned.
		/// </summary>
		public bool ReturnFullObjects { get; set; }

	}
}
