using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	public class HttpClientSaRootDelegatingHandler : DelegatingHandler
	{

		public HttpClientSa Client { get; set; }

		public HttpClientSaRootDelegatingHandler(HttpClientSa client)
		{
			Client = client;
		}

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken
		)
		{
			var response = await Client.SendAsync(request, cancellationToken);
			return response;
		}

	}
}
