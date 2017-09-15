using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ucsb.Sa.Enterprise.ClientExtensions;

namespace Ucsb.Sa.Enterprise.MvcExtensions
{
	public static class HttpCallRepeater
	{

		public async static Task<HttpResponseMessage> Repeat(int callId)
		{
			var container = HttpCallSearcher.GetFullLog(callId);
			if(container.Call == null)
			{
				return null;
			}

			return await Repeat(container.Call).ConfigureAwait(false);
		}

		public async static Task<HttpResponseMessage> Repeat(HttpCall call)
		{
			var headerLines = call.RequestHeader.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			var headers = new Dictionary<string,string>();
			foreach(var line in headerLines)
			{
				var i = line.IndexOf('=');
				var key = line.Substring(0, i);
				var value = line.Substring(i + 1, line.Length - i - 1);

				//	headers that cannot be reused
				switch(key.ToLower())
				{
					case "host":
					case "connection": continue;
				}

				headers.Add(key, value);
			}

			var uri = new Uri(call.Uri);
			var basepath = uri.Scheme + "://" + uri.Host;

			using(var client = new HttpClientSa(basepath, headers))
			{
                var method = new HttpMethod(call.Method);

				var response = await client.Execute(uri.PathAndQuery, method, call.RequestBody).ConfigureAwait(false);

				return response;
			}
		}

	}
}
