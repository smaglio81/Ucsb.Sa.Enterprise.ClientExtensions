using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Tests
{
	public interface IOverrideDefaultConfigTestService
	{
		Task<string> GetAsync();
	}

	public class OverrideDefaultConfigTestProxy: HttpClientSa, IOverrideDefaultConfigTestService
	{

		public static HttpClientSaConfiguration DefaultConfig = new HttpClientSaConfiguration()
		{
			Name = "OverrideDefaultConfigTest",
			BaseAddress = "http://registrar.{env}.sa.ucsb.edu/webservices/students",
			Headers = new Dictionary<string, string>() { { "authenticate", "encodedString" } }
		};

		public OverrideDefaultConfigTestProxy() : base(DefaultConfig) {}

		public async Task<string> GetAsync()
		{
			await Task.Delay(1);
			return "actualresult";
		}
	}
}
