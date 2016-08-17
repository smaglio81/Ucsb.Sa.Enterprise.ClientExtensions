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
	public class HttpClientConfigurationElement : ConfigurationElement
	{

		#region variables

		private const string __ConfigExceptionMessage =
			"ClientExtensions, HttpClientConfigurationElement.{0} property is not set. " +
			"Please set the necessary value within the configuration file. This should be " +
			"in the <clientExtensions>/<httpClients> config section with a key/value pair in the form " +
			"<httpClient ... {1}=\"{2}\" .../>.";

		#endregion

		#region public properties

		/// <summary>
		/// A unique name for this session transfer handler definition.
		/// </summary>
		[ConfigurationProperty("name")]
		public string Name
		{
			get
			{
				var result = (string)this["name"];
				if (string.IsNullOrEmpty(result))
				{
					throw new ConfigurationErrorsException(
						string.Format(
							__ConfigExceptionMessage,
							"Name",
							"name",
							@"unique name"
						)
					);
				}

				return result;
			}
			set { this["name"] = value; }
		}

		/// <summary>
		/// The base address of the webservice endpoint. This can be blank.
		/// </summary>
		[ConfigurationProperty("baseAddress")]
		public string BaseAddress
		{
			get
			{
				var result = (string)this["baseAddress"];
				return result;
			}
			set { this["baseAddress"] = value; }
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

		[ConfigurationProperty("traceLevel", DefaultValue = "")]
		private string TraceLevelString
		{
			get { return (string)this["traceLevel"]; }
			set { this["traceLevel"] = value; }
		}

		/// <summary>
		/// Determines if the client will be made as a singleton. A singleton
		/// will keep a persistent connection with the server between calls. This
		/// can be used to allow for multiple calls to be run against a single server.
		/// (If the server supports it)
		/// </summary>
		public bool IsSingleton
		{
			get
			{
				bool singleton = true;
				bool.TryParse(SingletonString, out singleton);
				return singleton;
			}
		}

		[ConfigurationProperty("singleton", DefaultValue = "true")]
		private string SingletonString
		{
			get { return (string)this["singleton"]; }
			set { this["singleton"] = value; }
		}

		/// <summary>
		/// This will configure the HttpClientSa client to convert .NET's
		/// PascalCase property names to json's camelCase property names.
		/// FirstName --> firstName
		/// (Default is false)
		/// </summary>
		public bool SerializeToCamelCase
		{
			get
			{
				bool serializeToCamelCase = false;
				bool.TryParse(SerializeToCamelCaseString, out serializeToCamelCase);
				return serializeToCamelCase;
			}
		}

		[ConfigurationProperty("serializeToCamelCase", DefaultValue = "true")]
		private string SerializeToCamelCaseString
		{
			get { return (string)this["serializeToCamelCase"]; }
			set { this["serializeToCamelCase"] = value; }
		}

		/// <summary>
		/// This will configure the HttpClientSa client to ignore adding implicit
		/// MS DTC transactions identifiers to request headers.
		/// (Default is false)
		/// </summary>
		public bool IgnoreImplicitTransactions
		{
			get
			{
				bool ignoreImplicitTransactions = false;
				bool.TryParse(IgnoreImplicitTransactionsString, out ignoreImplicitTransactions);
				return ignoreImplicitTransactions;
			}
		}

		[ConfigurationProperty("ignoreImplicitTransactions", DefaultValue = "false")]
		private string IgnoreImplicitTransactionsString
		{
			get { return (string)this["ignoreImplicitTransactions"]; }
			set { this["ignoreImplicitTransactions"] = value; }
		}

		/// <summary>
		/// Provides access to the container information in the section.
		/// </summary>
		[ConfigurationProperty("", IsDefaultCollection = true, IsKey = false, IsRequired = false)]
		[ConfigurationCollection(typeof(HeaderConfigurationElementCollection), AddItemName = "header")]
		public HeaderConfigurationElementCollection Headers
		{
			get
			{
				return (HeaderConfigurationElementCollection)this[""];
			}
		}

		#endregion

	}
}
