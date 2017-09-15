using System;
using System.Collections.Generic;
using System.Linq;
using Ucsb.Sa.Enterprise.ClientExtensions;
using Ucsb.Sa.Enterprise.ClientExtensions.Data;

namespace Ucsb.Sa.Enterprise.MvcExtensions
{
	/// <summary>
	/// Handles functions for performing searches against the database and filtering the results
	/// returned by a search against the database.
	/// </summary>
	public static class HttpCallSearcher
	{

		public static List<HttpCallLogDtoContainer> GetErrors(string uriPattern, DateTime? start, DateTime? end)
		{
			var parameters = new HttpCallSearchParameters()
			{
				UriPattern = uriPattern,
				Start = start,
				End = end,
				ErrorsOnly = true
			};
			return Search(parameters);
		}

		/// <summary>
		/// Gets the full log (call and error) for a given <paramref name="callId" />.
		/// </summary>
		/// <param name="callId">The call identifier.</param>
		/// <returns>The full log (call and error) for a given <paramref name="callId" />.</returns>
		public static HttpCallLogContainer GetFullLog(int callId)
		{
			var container = new HttpCallLogContainer();

			using(var db = new InstrumentationDbContext())
			{
				container.Call = db.Calls.FirstOrDefault(i => i.CallId == callId);
				container.Error = db.Errors.FirstOrDefault(i => i.CallId == callId);
			}

			return container;
		}

		public static List<HttpCallLogDtoContainer> Search(
			HttpCallSearchParameters parameters
		) {
			//	validation
			if(parameters.UriPattern != null)
			{
				var up = parameters.UriPattern;
				if (up.StartsWith("%")) { up = up.Substring(1, up.Length - 1); }
				if (up.EndsWith("%")) { up = up.Substring(0, up.Length - 1); }
				parameters.UriPattern = up;
			}

			//	searching
			using (var db = new InstrumentationDbContext())
			{
				//	if the search is to be filtered by only results with errors, then search on the errors
				//	first; as the table is smaller and easier to search through.
				var filterIds = new int[] {};
				if (parameters.ErrorsOnly)
				{
					var preqry = db.Errors.Where(i => true);

					if (parameters.UriPattern != null) { preqry = preqry.Where(i => i.Uri.Contains(parameters.UriPattern)); }
					if (parameters.Start != null) { preqry = preqry.Where(i => i.RequestDate >= parameters.Start); }
					if (parameters.End != null) { preqry = preqry.Where(i => i.RequestDate <= parameters.End); }

					filterIds = preqry.Select(i => i.CallId).ToArray();

					//	nothing to return
					if(filterIds.Count() == 0) { return new List<HttpCallLogDtoContainer>(); }
				}

				//	the Calls table query
				var qry = db.Calls.Where(i => true);

				if (filterIds.Count() > 0) { qry = qry.Where(i => filterIds.Contains(i.CallId)); }
				if (parameters.CallIds.Count > 0) { qry = qry.Where(i => parameters.CallIds.Contains(i.CallId)); }
				if (parameters.ServerNames.Count > 0) { qry = qry.Where(i => parameters.ServerNames.Contains(i.Server)); }
				if (parameters.IPs.Count > 0) { qry = qry.Where(i => parameters.IPs.Contains(i.IP)); }
				if (parameters.UriPattern != null) { qry = qry.Where(i => i.Uri.Contains(parameters.UriPattern)); }
				if (parameters.Start != null) { qry = qry.Where(i => i.RequestDate >= parameters.Start); }
				if (parameters.End != null) { qry = qry.Where(i => i.RequestDate <= parameters.End); }
				if (parameters.MinTimeDiff != null) { qry = qry.Where(i => i.TimeDiff >= parameters.MinTimeDiff); }
				if (parameters.MaxTimeDiff != null) { qry = qry.Where(i => i.TimeDiff <= parameters.MaxTimeDiff); }
				if (parameters.Direction != null) { qry = qry.Where(i => parameters.Direction == i.Direction); }
				if (parameters.StatusCodes.Count > 0) { qry = qry.Where(i => parameters.StatusCodes.Contains(i.StatusCode)); }
				if (parameters.Methods.Count > 0) { qry = qry.Where(i => parameters.Methods.Contains(i.Method)); }

				List<HttpCallDto> calls = new List<HttpCallDto>();
				if (parameters.MetadataFilter == null)
				{
					calls = GetCallDtos(qry, parameters);
				} else
				{
					if(parameters.ReturnFullObjects)
					{
						foreach (var call in qry)
						{
							var match = parameters.MetadataFilter(call.Metadata);
							if (match) { calls.Add(call); }
						}
					} else
					{
						var dtoQry = SelectCallDto(qry);
						foreach(var call in dtoQry)
						{
							var match = parameters.MetadataFilter(call.Metadata);
							if(match) { calls.Add(call); }
						}
					}
				}

				//	pull back matching entries fom the Errors tables
				var ids = calls.Select(i => i.CallId).ToArray();
				var eqry = db.Errors.Where(i => ids.Contains(i.CallId));
				var errors = new List<HttpErrorDto>();
				if (parameters.ReturnFullObjects) { errors.AddRange(eqry.ToList()); }
				else { errors.AddRange(SelectErrorDto(eqry).ToList()); }

				// combine the results
				var results = new List<HttpCallLogDtoContainer>();
				foreach (var call in calls)
				{
					var error = errors.FirstOrDefault(i => i.CallId == call.CallId);

					var container = new HttpCallLogDtoContainer();
					container.Call = call;
					container.Error = error;

					results.Add(container);
				}

				return results;
			}	//	using(var db = ...
		}

		internal static List<HttpCallDto> GetCallDtos(
			IQueryable<HttpCall> qry,
			HttpCallSearchParameters parameters
		) {
			var calls = new List<HttpCallDto>();

			if (parameters.ReturnFullObjects)
			{
				calls.AddRange(qry.Select(i => i).ToList());
			}
			else
			{
				//	we need to list out the column names directly to allow EF to create
				//	a smaller column retrieval list in the sql query.
				calls = SelectCallDto(qry).ToList();
			}

			return calls;
		}

		internal static IQueryable<HttpCallDto> SelectCallDto(IQueryable<HttpCall> qry)
		{
			return qry.Select(i => new HttpCallDto
				{
					CallId = i.CallId,
					Server = i.Server,
					IP = i.IP,
					Uri = i.Uri,
					RequestDate = i.RequestDate,
					ResponseDate = i.ResponseDate,
					TimeDiff = i.TimeDiff,
					Direction = i.Direction,
					StatusCode = i.StatusCode,
					Method = i.Method,
					Metadata = i.Metadata
				});
		}

		internal static IQueryable<HttpErrorDto> SelectErrorDto(IQueryable<HttpError> qry)
		{
			return qry.Select(i => new HttpErrorDto
			{
				ErrorId = i.ErrorId,
				CallId = i.CallId,
				Type = i.Type,
				Message = i.Message,
				Source = i.Source,
				TargetSite = i.TargetSite
			});
		}

	}
}
