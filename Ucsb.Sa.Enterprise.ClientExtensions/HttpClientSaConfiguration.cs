using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// This represents the possible configuration values for clients that are
	/// configured with the manager.
	/// </summary>
	public class HttpClientSaConfiguration
	{

		internal IDictionary<string, string> _Headers;

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSaConfiguration"/> class.
		/// </summary>
		public HttpClientSaConfiguration()
		{
			Headers = new Dictionary<string, string>();
			SerializeToCamelCase = true;
			IgnoreImplicitTransactions = false;
			DelegatingHandlers = new List<DelegatingHandlerDefinition>();
		}

		/// <summary>
		/// Gets or sets the unique name of the client.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the base address for the client. The base address can use an {env} substitution.
		/// </summary>
		public string BaseAddress { get; set; }

		/// <summary>
		/// The trace level for this client.
		/// </summary>
		public HttpClientSaTraceLevel TraceLevel { get; set; }

		/// <summary>
		/// Determines if the client will be made as a singleton. A singleton
		/// will keep a persistent connection with the server between calls. This
		/// can be used to allow for multiple calls to be run against a single server.
		/// (If the server supports it)
		/// </summary>
		public bool IsSingleton { get; set; }

		/// <summary>
		/// This will configure the HttpClientSa client to convert .NET's
		/// PascalCase property names to json's camelCase property names.
		/// FirstName --> firstName
		/// </summary>
		public bool SerializeToCamelCase { get; set; }

		/// <summary>
		/// This will configure the HttpClientSa client to ignore adding implicit
		/// MS DTC transactions identifiers to request headers.
		/// (Default is false)
		/// </summary>
		public bool IgnoreImplicitTransactions { get; set; }

		/// <summary>
		/// Gets or sets the post database logged callback.
		/// </summary>
		public Action<HttpCall> PostDbLogged { get; set; }

		/// <summary>
		/// Gets or sets the default headers to add to every call made by the client.
		/// </summary>
		public IDictionary<string, string> Headers
		{
			get { return _Headers; }
			set { if (value != null) { _Headers = value; } }
		}

		/// <summary>
		/// Gets or sets the list of <see cref="System.Net.Http.DelegatingHandler" /> objects
		/// to add into the <see cref="HttpClientSa" /> instance.
		/// </summary>
		public IList<DelegatingHandlerDefinition> DelegatingHandlers { get; set; }

		public override bool Equals(object obj)
		{
			return obj.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode()
		{
			var hashCode = 0;
			foreach(var key in Headers.Keys)
			{
				hashCode += key.GetHashCode() + Headers[key].GetHashCode();
			}
			hashCode += BaseAddress.GetHashCode();
			return hashCode;
		}
	}
}
