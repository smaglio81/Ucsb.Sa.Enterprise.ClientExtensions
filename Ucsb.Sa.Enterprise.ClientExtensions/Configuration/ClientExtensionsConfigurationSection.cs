using System;
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
    public class ClientExtensionsConfigurationSection : ConfigurationSection
	{

		#region variables

		private static readonly string __ConfigurationNotSet =
			"ClientExtensions HttpClientSaManager could not find a configuration section within the " +
			"applications .config file. This can be accomplished by adding" + Environment.NewLine +
			Environment.NewLine +
			"<configSections>" + Environment.NewLine +
            "	<section name=\"clientExtensions\" type=\"Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions\" />" + Environment.NewLine +
			"</configSections>" + Environment.NewLine +
			Environment.NewLine +
			"<connectionStrings>" + Environment.NewLine +
			"	<connectionString name=\"Instrumentation\" connectionString=\"Initial Catalog=Instrumentation;Data Source=instrumentation.sql.{env}.sa.ucsb.edu,2433;Integrated Security=SSPI;\" />" + Environment.NewLine +
			"</connectionStrings>" + Environment.NewLine +
			Environment.NewLine +
			"<clientExtensions>" + Environment.NewLine +
			"	<httpClients default=\"client1\" traceLevel=\"ALL|[NONE]\">" + Environment.NewLine +
            "		<httpClient name=\"client1\" baseAddress=\"http://jsonplaceholder.typicode.com\" traceLevel=\"ALL|[NONE]\" singleton=\"[true]|false\">" + Environment.NewLine +
			"			<header name=\"h1\" value=\"v1\" />" + Environment.NewLine +
			"		</httpClient>" + Environment.NewLine +
			"	</applications>" + Environment.NewLine +
			"</clientExtensions>" + Environment.NewLine +
			Environment.NewLine +
			"to your applications .config file. ";

		private static ClientExtensionsConfigurationSection _Configuration;

		#endregion

		#region static properties

		/// <summary>
		/// Retrieves an new instance of a <see cref="ClientExtensionsConfigurationSection" /> populated
		/// with the current information held within the application configuration file.
		/// </summary>
		public static ClientExtensionsConfigurationSection Configuration
		{
			get
			{
				if (_Configuration == null)
				{
					_Configuration =
						(ClientExtensionsConfigurationSection)ConfigurationManager.GetSection("clientExtensions");


					if (_Configuration == null)
						throw new ConfigurationErrorsException(__ConfigurationNotSet);
				}

				return _Configuration;
			}
		}

		#endregion

		#region properties

		/// <summary>
		/// Provides access to the container information in the section.
		/// </summary>
		[ConfigurationProperty("httpClients")]
		[ConfigurationCollection(typeof(HttpClientsConfigurationElementCollection), AddItemName = "httpClient")]
		public HttpClientsConfigurationElementCollection HttpClients
		{
			get
			{
				return (HttpClientsConfigurationElementCollection)this["httpClients"];
			}
		}

		#endregion
	}
}
