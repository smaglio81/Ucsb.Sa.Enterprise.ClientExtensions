using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Tests
{
	public class DummyHandler : DelegatingHandler
	{

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken
		)
		{
			var response = await base.SendAsync(request, cancellationToken);
			return response;
		}
	}
}
