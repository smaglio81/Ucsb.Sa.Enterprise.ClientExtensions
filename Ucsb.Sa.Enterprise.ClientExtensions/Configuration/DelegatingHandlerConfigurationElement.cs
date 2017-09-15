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
	public class DelegatingHandlerConfigurationElement : ConfigurationElement
	{

		#region variables

		private const string __ConfigExceptionMessage =
			"ClientExtensions, HttpClientConfiguration's DelegatedHandlerConfigurationElement.{0} property is not set. " +
			"Please set the necessary value within the configuration file. This should be " +
			"in the <clientExtensions>/<httpClients>/<httpClient> config section with a key/value pair in the form " +
			"<delegatingHandler ... {1}=\"{2}\" .../>.";

		#endregion

		#region override properties

		public override bool IsReadOnly()
		{
			return false;
		}

		#endregion

		#region public properties

		/// <summary>
		/// A unique class name within an assembly. This class will be loaded as
		/// a delegated handler on the <see cref="HttpClientSa" /> instance.
		/// </summary>
		[ConfigurationProperty("class")]
		public string Class
		{
			get
			{
				var result = (string)this["class"];
				if (string.IsNullOrEmpty(result))
				{
					throw new ConfigurationErrorsException(
						string.Format(
							__ConfigExceptionMessage,
							"Class",
							"class",
							@"class name"
						)
					);
				}

				return result;
			}
			set { this["class"] = value; }
		}

		/// <summary>
		/// A value to go with the unique header
		/// </summary>
		[ConfigurationProperty("assembly")]
		public string Assembly
		{
			get
			{
				var result = (string)this["assembly"];
				if (string.IsNullOrEmpty(result))
				{
					throw new ConfigurationErrorsException(
						string.Format(
							__ConfigExceptionMessage,
							"Assembly",
							"assembly",
							@"assembly name (without version, culture, or publicKeyToken)"
						)
					);
				}

				return result;
			}
			set { this["assembly"] = value; }
		}

		#endregion

	}
}
