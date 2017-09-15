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
	///		<section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsRedisConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions.Redis" />
	/// </configSections>
	/// 
	/// <connectionStrings>
	///		<add name="Instrumentation" connectionString="Initial Catalog=Instrumentation;Data Source=instrumentation.sql.{env}.sa.ucsb.edu,2433;Integrated Security=SSPI;" />
	/// </connectionStrings>
	/// 
	/// <clientExtensions redisHost="appcaching.dev.sa.ucsb.edu">
	///		<httpClients default="client1" traceLevel="ALL|[NONE]">
	///			<httpClient name="client1" baseAddress="http://jsonplaceholder.typicode.com" traceLevel="ALL|[NONE]" singleton="[true]|false">
	///				<header name="h1" value="v1" />
	///			</httpClient>
	///		</httpClients>
	/// </clientExtensions>
	/// </code>
	/// </example>
	public class ClientExtensionsRedisConfigurationSection : ClientExtensionsConfigurationSection
	{

		#region variables

		private static readonly string __ConfigurationNotSet =
			"ClientExtensions.Redis HttpClientSaManager could not find a configuration section within the " +
			"applications .config file. This can be accomplished by adding" + Environment.NewLine +
			Environment.NewLine +
			"<configSections>" + Environment.NewLine +
			"	<section name=\"clientExtensions\" type=\"Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsRedisConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions.Redis\" />" + Environment.NewLine +
			"</configSections>" + Environment.NewLine +
			Environment.NewLine +
			"<connectionStrings>" + Environment.NewLine +
			"	<connectionString name=\"Instrumentation\" connectionString=\"Initial Catalog=Instrumentation;Data Source=instrumentation.sql.{env}.sa.ucsb.edu,2433;Integrated Security=SSPI;\" />" + Environment.NewLine +
			"</connectionStrings>" + Environment.NewLine +
			Environment.NewLine +
			"<clientExtensions redisHost=\"appcaching.dev.sa.ucsb.edu\">" + Environment.NewLine +
			"	<httpClients default=\"client1\" traceLevel=\"ALL|[NONE]\">" + Environment.NewLine +
			"		<httpClient name=\"client1\" baseAddress=\"http://jsonplaceholder.typicode.com\" traceLevel=\"ALL|[NONE]\" singleton=\"[true]|false\">" + Environment.NewLine +
			"			<header name=\"h1\" value=\"v1\" />" + Environment.NewLine +
			"		</httpClient>" + Environment.NewLine +
			"	</applications>" + Environment.NewLine +
			"</clientExtensions>" + Environment.NewLine +
			Environment.NewLine +
			"to your applications .config file. ";

		private static ClientExtensionsRedisConfigurationSection _Configuration;

		#endregion

		#region static properties

		/// <summary>
		/// Retrieves an new instance of a <see cref="ClientExtensionsConfigurationSection" /> populated
		/// with the current information held within the application configuration file.
		/// </summary>
		public static ClientExtensionsRedisConfigurationSection Configuration
		{
			get
			{
				if (_Configuration == null)
				{
					_Configuration =
						(ClientExtensionsRedisConfigurationSection)ConfigurationManager.GetSection("clientExtensions");


					if (_Configuration == null)
						throw new ConfigurationErrorsException(__ConfigurationNotSet);
				}

				return _Configuration;
			}
		}

		public static bool ConfigurationSectionExists
		{
			get
			{
				_Configuration =
					(ClientExtensionsRedisConfigurationSection)ConfigurationManager.GetSection("clientExtensions");

				return _Configuration != null;
			}
		}

		#endregion

		#region properties

		/// <summary>
		/// The host name of redis server to use. This can be used to override the
		/// default functionality of using the correct redis server for the environment.
		/// </summary>
		[ConfigurationProperty("redisHost")]
		public string RedisHost
		{
			get { return (string)this["redisHost"]; }
			set { this["redisHost"] = value; }
		}

		#endregion
	}
}
