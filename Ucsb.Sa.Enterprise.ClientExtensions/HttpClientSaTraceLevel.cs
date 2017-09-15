using System;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// Different types of tracing that the service can perform.
	/// </summary>
	public enum HttpClientSaTraceLevel
	{
		Undefined,
		None,
		Error,
		All
	}

	/// <summary>
	/// Parses strings to <see cref="HttpClientSaTraceLevel" /> enum value.
	/// </summary>
	public static class HttpClientSaTraceLevelParser
	{
		/// <summary>
		/// Parses strings to <see cref="HttpClientSaTraceLevel" /> enum value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">If the value is null.</exception>
		/// <exception cref="ArgumentException">If the value is invalid.</exception>
		public static HttpClientSaTraceLevel Parse(string value)
		{
			if(value == null)
			{
				throw new ArgumentNullException(paramName: "value");
			}

			value = value.Trim().ToLower();
			switch(value)
			{
				case "": return HttpClientSaTraceLevel.Undefined;
				case "none": return HttpClientSaTraceLevel.None;
				case "error": return HttpClientSaTraceLevel.Error;
				case "all": return HttpClientSaTraceLevel.All;
			}

			var message = string.Format(
					"The value given ('{0}') to parse into a HttpClientSaTraceLevel enum is not valid. Valid " +
					"values are 'All' and 'None'.",
					value
				);
			throw new ArgumentException(message);
		}
	}

}
