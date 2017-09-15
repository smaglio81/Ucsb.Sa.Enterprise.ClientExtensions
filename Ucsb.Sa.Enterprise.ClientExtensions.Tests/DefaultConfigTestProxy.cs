using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Tests
{
	public interface IDefaultConfigTestService
	{
		Task<string> GetAsync();
	}

	public class DefaultConfigTestProxy: HttpClientSa, IDefaultConfigTestService
	{

		public static HttpClientSaConfiguration DefaultConfig = new HttpClientSaConfiguration()
		{
			Name = "DefaultConfigTest",
			BaseAddress = "http://registrar.{env}.sa.ucsb.edu/webservices/students/",
			Headers = new Dictionary<string, string>() { { "Authorization", "Basic encodedString" } },
			DelegatingHandlers = new List<DelegatingHandlerDefinition>()
			{
				new DelegatingHandlerDefinition() { ClassName = "DummyHandler", AssemblyName = "Ucsb.Sa.Enterprise.ClientExtensions.Tests" }
			}
		};

		public DefaultConfigTestProxy() : base(DefaultConfig) {}

		public async Task<string> GetAsync()
		{
			await Task.Delay(1);
			return "actualresult";
		}
	}
}
