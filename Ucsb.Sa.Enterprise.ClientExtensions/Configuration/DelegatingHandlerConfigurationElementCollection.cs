using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Configuration
{
	/// <summary>
	/// Used by the client within the application configuration file (App.Config/Web.Config), to convert
	/// a configuration section named <c>clientExtensions/httpClients/httpClient</c> to create a instances of
	/// <see cref="HttpClientSa" />.
	/// </summary>
	/// <example>
	/// <code lang="xml">
	/// <configSections>
	///		<section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions" />
	/// </configSections>
	/// 
	/// <connectionStrings>
	///		<add name="Instrumentation" connectionString="Initial Catalog=Instrumentation;Data Source=instrumentation.sql.{env}.sa.ucsb.edu,2433;Integrated Security=SSPI;" />
	/// </connectionStrings>
	/// 
	/// <clientExtensions>
	///		<httpClients default="client1" traceLevel="ALL|[NONE]">
	///			<httpClient name="client1" baseAddress="http://jsonplaceholder.typicode.com" traceLevel="ALL|[NONE]" singleton="[true]|false">
	///				<delegatingHandler class="DummyHandler" assembly="Ucsb.Sa.Enterprise.ClientExtensions.Tests" />
	///			</httpClient>
	///		</httpClients>
	/// </clientExtensions>
	/// </code>
	/// </example>
	public class DelegatingHandlerConfigurationElementCollection : ConfigurationElementCollection
	{

		protected override ConfigurationElement CreateNewElement()
		{
			return new DelegatingHandlerConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			var handler = (DelegatingHandlerConfigurationElement)element;
			var key = string.Format("{0}, {1}", handler.Class, handler.Assembly);
			return key;
		}

	}
}
