using System.Configuration;

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
    ///				<header name="h1" value="v1" />
    ///			</httpClient>
    ///		</httpClients>
    /// </clientExtensions>
    /// </code>
    /// </example>
    public class HttpClientsConfigurationElementCollection : ConfigurationElementCollection
	{

		#region Overrides of ConfigurationElementCollection

		protected override ConfigurationElement CreateNewElement()
		{
			return new HttpClientConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			var handler = (HttpClientConfigurationElement)element;
			return handler.Name;
		}

		#endregion

		#region properties

		/// <summary>
		/// Defines the name of the default handler to use if
		/// <see cref="HttpClientSaManager.NewClient()" /> is invoked with
		/// no value or an empty string.
		/// </summary>
		[ConfigurationProperty("default", DefaultValue = "")]
		public string DefaultName
		{
			get { return (string)this["default"]; }
			set { this["default"] = value; }
		}

		/// <summary>
		/// Defines the name of the default handler to use if
		/// <see cref="HttpClientSaManager.NewClient()" /> is invoked with
		/// no value or an empty string.
		/// </summary>
		public HttpClientSaTraceLevel TraceLevel
		{
			get
			{
				return HttpClientSaTraceLevelParser.Parse(TraceLevelString);
			}
		}

		#endregion

		#region internal properties

		[ConfigurationProperty("traceLevel", DefaultValue = "")]
		private string TraceLevelString
		{
			get { return (string)this["traceLevel"]; }
			set { this["traceLevel"] = value; }
		}

		#endregion

	}
}
