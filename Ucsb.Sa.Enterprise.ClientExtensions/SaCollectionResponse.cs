using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// A CQRS designed response message for paged results.
	/// </summary>
	public class SaCollectionResponse
	{
		/// <summary>
		/// This is a message about the status of the response. In general this will be "OK". But, it can
		/// be customized for error details.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The uri for the first page of results.
		/// </summary>
		public string FirstPageUri { get; set; }

		/// <summary>
		/// The uri for the next page of results.
		/// </summary>
		public string NextPageUri { get; set; }

		/// <summary>
		/// The uri for the last page of results.
		/// </summary>
		public string LastPageUri { get; set; }

		/// <summary>
		/// The page of results.
		/// </summary>
		public SaPageResponse Page { get; set; }

	}

	/// <summary>
	/// A page of results. The extra data makes it easier to use in UI clients.
	/// </summary>
	public class SaPageResponse
	{
		/// <summary>
		/// The number of results per page.
		/// </summary>
		public int PageSize { get; set; }

		/// <summary>
		/// The number of the first result in the set. For example, if this is page
		/// 2, with 20 results per page, then the Start index should be 21.
		/// </summary>
		public int Start { get; set; }

		/// <summary>
		/// The array of results
		/// </summary>
		public IEnumerable Data { get; set; }

		/// <summary>
		/// The total number of results in the complete set
		/// </summary>
		public int TotalCount { get; set; }

		/// <summary>
		/// The total number of pages in the result set
		/// </summary>
		public int TotalPageCount { get; set; }

		/// <summary>
		/// The current page number in the result set.
		/// </summary>
		public int CurrentPageNo { get; set; }
	}
}
