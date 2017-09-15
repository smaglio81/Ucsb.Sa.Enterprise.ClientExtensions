using System;
using System.Configuration;
using System.Text;

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
    public class HeaderConfigurationElement : ConfigurationElement
	{

		#region variables

		private const string __ConfigExceptionMessage =
			"ClientExtensions, HttpClientConfiguration's HeaderConfigurationElement.{0} property is not set. " +
			"Please set the necessary value within the configuration file. This should be " +
			"in the <clientExtensions>/<httpClients>/<httpClient> config section with a key/value pair in the form " +
			"<header ... {1}=\"{2}\" .../>.";

		#endregion

		#region override properties

		public override bool IsReadOnly()
		{
			return false;
		}

		#endregion

		#region public properties

		/// <summary>
		/// A unique name for this session transfer handler definition. The awful thing
		/// that is that the HTTP standards allows for multiple headers with the same
		/// name. Becuase I setup the element key in <see cref="HttpClientConfigurationElement" />
		/// to use the name as the unique key, I think it now _has_ to be unique.
		/// So, this could retrict developers in a way we don't want to.
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
							@"http header name"
						)
					);
				}

				return result;
			}
			set { this["name"] = value; }
		}

		/// <summary>
		/// A value to go with the unique header
		/// </summary>
		[ConfigurationProperty("value")]
		public string ValueConfig
		{
			get
			{
				var result = (string)this["value"];
				if (string.IsNullOrEmpty(result))
				{
					throw new ConfigurationErrorsException(
						string.Format(
							__ConfigExceptionMessage,
							"Value",
							"value",
							@"the value"
						)
					);
				}

				return result;
			}
			set { this["value"] = value; }
		}

	    public string Value
	    {
		    get
		    {
			    if(string.IsNullOrWhiteSpace((string)this["value"]) == false) { return ValueConfig; }
				if(Name.ToLower() != "authorization") { return ValueConfig; }

				//	base64 encode
			    var pair = Username + ":" + Password;
			    var pairBytes = Encoding.ASCII.GetBytes(pair);
			    var hash = Convert.ToBase64String(pairBytes);
			    var basic = "Basic " + hash;
			    ValueConfig = basic;

			    return ValueConfig;
		    }
			set { ValueConfig = value; }
	    }

		/// <summary>
		/// The username to base64 encode when using Basic Authorization. Only use this when the name
		/// is Authorization
		/// </summary>
		[ConfigurationProperty("username")]
		public string Username
		{
			get
			{
				var result = (string)this["username"];
				if (string.IsNullOrEmpty(result))
				{
					throw new ConfigurationErrorsException(
						string.Format(
							__ConfigExceptionMessage,
							"Username",
							"username",
							@"the username to base64 encode with Basic Authorization"
						)
					);
				}

				return result;
			}
			set { this["username"] = value; }
		}



		/// <summary>
		/// The password to base64 encode when using Basic Authorization. Only use this when the name
		/// is Authorization
		/// </summary>
		[ConfigurationProperty("password")]
		public string Password
		{
			get
			{
				var result = (string)this["password"];
				if (string.IsNullOrEmpty(result))
				{
					throw new ConfigurationErrorsException(
						string.Format(
							__ConfigExceptionMessage,
							"Password",
							"password",
							@"the password to base64 encode with Basic Authorization"
						)
					);
				}

				return result;
			}
			set { this["password"] = value; }
		}

		#endregion

	}
}
