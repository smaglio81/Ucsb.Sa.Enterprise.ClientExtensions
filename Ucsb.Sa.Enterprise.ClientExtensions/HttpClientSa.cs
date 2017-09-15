using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Ucsb.Sa.Enterprise.ClientExtensions.Data;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// <see cref="HttpClient" /> wrapper which adds extension methods which are easier to call and
	/// retreive the returned value from web services.
	/// </summary>
	public class HttpClientSa : HttpClient
	{

		#region variables/enums/structs
		
		/// <summary>
		/// Which calls will be traced to the database.
		/// </summary>
		public HttpClientSaTraceLevel TraceLevel = HttpClientSaTraceLevel.None;

		/// <summary>
		/// All callback function to be used after a call is logged to the db.
		/// </summary>
		public Action<HttpCall> PostDbLogged = null;

		/// <summary>
		/// All Callback function to be used in case of exception occur
		/// </summary>
		public Action<HttpCall> PostException = null;

		/// <summary>
		/// The last HTTP response message retrieved using the synchronous overload
		/// methods.
		/// </summary>
		public HttpResponseMessage LastHttpResponseMessage = null;

		private HeaderComparer RequestHeaderComparer = new HeaderComparer();
		/// <summary>
		/// The default contract resolver used by JSON.net. This will translate
		/// property names exactly as they are (FirstName --> FirstName).
		/// </summary>
		internal JsonSerializerSettings _JsonNetSerializerSettings =
			new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

		/// <summary>
		/// Internal SerializeToCamelCase value. Used for the get property.
		/// </summary>
		internal bool SerializeToCamelCaseValue = true;

		/// <summary>
		/// Internal cache for lookup tables.
		/// </summary>
		internal Lazy<MemoryCache>	_Cache = new Lazy<MemoryCache>(() => { return new MemoryCache("HttpClientSa"); });

		/// <summary>
		/// This helps implement <see cref="DelegatingHandler" />.
		/// DelegatingHandler: https://msdn.microsoft.com/en-us/library/system.net.http.delegatinghandler(v=vs.118).aspx
		/// -> extends HttpMessageHandler:  https://msdn.microsoft.com/en-us/library/system.net.http.httpmessagehandler(v=vs.118).aspx
		/// HttpMessageInvoker: https://msdn.microsoft.com/en-us/library/system.net.http.httpmessageinvoker(v=vs.118).aspx
		/// -> Has constructor that takes HttpMessageHandler
		/// </summary>
		public HttpMessageHandler SendAsyncMessageHandler = null;

		#endregion

		#region properties

		/// <summary>
		/// Request Headers that will applied to calls made by this client.
		/// </summary>
		public IDictionary<string, string> RequestHeaders;

		/// <summary>
		/// This will configure the HttpClientSa client to convert .NET's
		/// PascalCase property names to json's camelCase property names.
		/// FirstName --> firstName
		/// (Default is false)
		/// </summary>
		public bool SerializeToCamelCase {
			get { return SerializeToCamelCaseValue; }
			set
			{
				if (SerializeToCamelCaseValue != value)
				{
					if (value)
					{
						JsonNetSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
					}
					else
					{
						JsonNetSerializerSettings.ContractResolver = new DefaultContractResolver();
					}
				}
			}
		}

		/// <summary>
		/// The Json.NET serializer settings.
		/// </summary>
		public JsonSerializerSettings JsonNetSerializerSettings
		{
			get { return _JsonNetSerializerSettings; }
			set { _JsonNetSerializerSettings = value; }
		}

		/// <summary>
		/// This will configure the HttpClientSa client to ignore adding implicit
		/// MS DTC transactions identifiers to request headers.
		/// (Default is false)
		/// </summary>
		public bool IgnoreImplicitTransactions { get; set; }

		/// <summary>
		/// Internal cache for lookup tables.
		/// </summary>
		internal MemoryCache Cache { get { return _Cache.Value; } }

		#endregion

		#region constructors

		static HttpClientSa()
		{
			// NOTE: this has to be updated as new TLS are released. I couldn't figure out
			//		 how to just say: "Always use the latest".
			ServicePointManager.SecurityProtocol = 
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSa"/> class.
		/// </summary>
		public HttpClientSa()
		{
			RequestHeaders = new Dictionary<string, string>();
			IgnoreImplicitTransactions = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSa"/> class.
		/// </summary>
		/// <param name="baseAddress">
		/// The base address. This value can be overloaded to be the name of an endpoint defined
		/// in the <see cref="HttpClientSaManager" />.
		/// </param>
		public HttpClientSa(string baseAddress) : this()
		{
			this.CheckBaseAddressAndConfigure(baseAddress);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSa" /> class. This configuration
		/// can use a url which uses the {env} environment substitution. If this substitution is used
		/// then an appSettings/add[name="environment"] value must be supplied.
		/// </summary>
		/// <param name="defaultConfig">
		/// A default configuration for the client which can use the {env} substitution. If this
		/// substitution is used then an appSettings/add[name="environment"] value must be supplied.
		/// </param>
		public HttpClientSa(HttpClientSaConfiguration defaultConfig)
		{
			this.CheckDefaultConfigAndConfigure(defaultConfig);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSa"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers to add to every call.</param>
		public HttpClientSa(string baseAddress, IDictionary<string,string> headers) :
			this(baseAddress)
		{
			this.CheckHeadersAndConfigure(headers);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSa"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers to add to every call.</param>
		/// <param name="traceLevel">The level of tracing.</param>
		public HttpClientSa(string baseAddress, IDictionary<string, string> headers, HttpClientSaTraceLevel traceLevel) :
			this(baseAddress, headers)
		{
			this.CheckTraceLevelAndConfigure(traceLevel);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSa"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers to add to every call.</param>
		/// <param name="traceLevel">The level of tracing.</param>
		/// <param name="postDbLogged">A call back to use if the call is logged to the db.</param>
		public HttpClientSa(
			string baseAddress,
			IDictionary<string, string> headers,
			HttpClientSaTraceLevel traceLevel,
			Action<HttpCall> postDbLogged
		) :
			this(baseAddress, headers, traceLevel)
		{
			this.CheckPostDbLoggedAndConfigure(postDbLogged);
		}

		#endregion

		#region public methods

		/// <summary>
		/// Gets the data object at the specified URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns>The string body of the returned result.</returns>
		public string Get(string url = "", string datatype = "json", Transaction transaction = null)
		{
			return GetAsyncAsString(url, datatype, transaction).Result;
		}

		/// /// <summary>
		/// Gets the data object at the specified URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns>The string body of the returned result.</returns>
		public T Get<T>(string url = "", string datatype = "json", Transaction transaction = null)
		{
			return GetAsync<T>(url, datatype, transaction).Result;
		}

		/// <summary>
		/// Gets the data object at the specified URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns>The string body of the returned result.</returns>
		public async Task<string> GetAsyncAsString(string url = "", string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Get, null, datatype, transaction).ConfigureAwait(false);
			var content = response.ResponseAsString();
			return content;
		}

		/// /// <summary>
		/// Gets the data object at the specified URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns>The string body of the returned result.</returns>
		public async Task<T> GetAsync<T>(string url = "", string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Get, null, datatype, transaction).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				TestInternalServerError(response);
				return default(T);
			}

			var content = response.ResponseAsString();
			return HttpResponseMessageExtensions.DeserializeHttpResponse<T>(content, datatype);
		}

		/// <summary>
		/// Gets a cached copy of the data for a given Get operation. This should be used with lookup tables.
		/// 
		/// http://blog.falafel.com/working-system-runtime-caching-memorycache/
		/// </summary>
		/// <typeparam name="T">The type of the value returned.</typeparam>
		/// <param name="url">The url to pull data from.</param>
		/// <param name="datatype">The expected format of the returned data (default: json)</param>
		/// <param name="policy">The cache item policy. (default: 6 hours absolute time)</param>
		/// <returns>The lookup table reqeusted.</returns>
		public virtual T GetCached<T>(string url, string datatype = "json", CacheItemPolicy policy = null)
		{
			//	http://blog.falafel.com/working-system-runtime-caching-memorycache/
			Func<T> valueFactory = () => {
				var response = Get<T>(url: url, datatype: datatype);
				return (T)response;
			};

			if (policy == null)
			{
				policy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.Add(TimeSpan.FromHours(6)) };
			}

			var newValue = new Lazy<T>(valueFactory);
			var oldValue = Cache.AddOrGetExisting(key: url, value: newValue, policy: policy) as Lazy<T>;
			try
			{
				return (oldValue ?? newValue).Value;
			}
			catch
			{
				// Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
				Cache.Remove(key: url);
				throw;
			}
		}

		/// <summary>
		/// Gets a cached copy of the data for a given Get operation. This should be used with lookup tables.
		/// 
		/// http://blog.falafel.com/working-system-runtime-caching-memorycache/
		/// </summary>
		/// <typeparam name="T">The type of the value returned.</typeparam>
		/// <param name="url">The url to pull data from.</param>
		/// <param name="datatype">The expected format of the returned data (default: json)</param>
		/// <param name="policy">The cache item policy. (default: 6 hours absolute time)</param>
		/// <returns>The lookup table reqeusted.</returns>
		public virtual async Task<T> GetCachedAsync<T>(string url, string datatype = "json", CacheItemPolicy policy = null)
		{
			//	http://blog.falafel.com/working-system-runtime-caching-memorycache/
			Func<Task<T>> valueFactory = async () => {
				var task = await GetAsync<T>(url: url, datatype: datatype).ConfigureAwait(false);
				return task;
			};

			if (policy == null)
			{
				policy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.Add(TimeSpan.FromHours(6)) };
			}

			var newValue = new AsyncLazy<T>(valueFactory);
			var oldValue = Cache.AddOrGetExisting(key: url, value: newValue, policy: policy) as AsyncLazy<T>;
			try
			{
				var val = (oldValue ?? newValue);
				return await val.Value.ConfigureAwait(false);
			}
			catch
			{
				// Handle cached lazy exception by evicting from cache. Thanks to Denis Borovnev for pointing this out!
				Cache.Remove(key: url);
				throw;
			}
		}

		/// <summary>
		/// Posts the data object to the specified URL. This is like an Update. It's easy to
		/// use, because all it really requires is the unique id of the object and the properties
		/// to update. Not everyone implements their API to be that smart, so be careful. Alot of
		/// developers will also overload the functionality to Insert an object if it doesn't
		/// exist.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public string Post(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			return PostAsyncAsString(url, data, datatype, transaction).Result;
		}

		/// <summary>
		/// Posts the data object to the specified URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public T Post<T>(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			return PostAsync<T>(url, data, datatype, transaction).Result;
		}

		/// <summary>
		/// Posts the data object to the specified URL. This is like an Update. It's easy to
		/// use, because all it really requires is the unique id of the object and the properties
		/// to update. Not everyone implements their API to be that smart, so be careful. Alot of
		/// developers will also overload the functionality to Insert an object if it doesn't
		/// exist.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public async Task<string> PostAsyncAsString(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Post, data, datatype, transaction).ConfigureAwait(false);
			var content = response.ResponseAsString();
			return content;
		}

		/// <summary>
		/// Posts the data object to the specified URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public async Task<T> PostAsync<T>(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Post, data, datatype, transaction).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				TestInternalServerError(response);
				return default(T);
			}

			var content = response.ResponseAsString();
			return HttpResponseMessageExtensions.DeserializeHttpResponse<T>(content, datatype);
		}

		/// <summary>
		/// Puts the data object at the specified URL.This is like an Insert. But, can be used
		/// to Update, where it overwrites all values. So, if a property is missing, that value
		/// will be set to null.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public string Put(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			return PutAsyncAsString(url, data, datatype, transaction).Result;
		}

		/// <summary>
		/// Puts the data object at the specified URL. This is like an Insert. But, can be used
		/// to Update, where it overwrites all values. So, if a property is missing, that value
		/// will be set to null.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public T Put<T>(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			return PutAsync<T>(url, data, datatype, transaction).Result;
		}

		/// <summary>
		/// Puts the data object at the specified URL.This is like an Insert. But, can be used
		/// to Update, where it overwrites all values. So, if a property is missing, that value
		/// will be set to null.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public async Task<string> PutAsyncAsString(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Put, data, datatype, transaction).ConfigureAwait(false);
			var content = response.ResponseAsString();
			return content;
		}

		/// <summary>
		/// Puts the data object at the specified URL. This is like an Insert. But, can be used
		/// to Update, where it overwrites all values. So, if a property is missing, that value
		/// will be set to null.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public async Task<T> PutAsync<T>(string url = "", object data = null, string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Put, data, datatype, transaction).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				TestInternalServerError(response);
				return default(T);
			}

			var content = response.ResponseAsString();
			return HttpResponseMessageExtensions.DeserializeHttpResponse<T>(content, datatype);
		}

		/// <summary>
		/// Deletes the specified object at the URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public string Delete(string url = "", string datatype = "json", Transaction transaction = null)
		{
			return DeleteAsyncAsString(url, datatype, transaction).Result;
		}

		/// <summary>
		/// Deletes the specified object at the URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public T Delete<T>(string url = "", string datatype = "json", Transaction transaction = null)
		{
			return DeleteAsync<T>(url, datatype, transaction).Result;
		}

		/// <summary>
		/// Deletes the specified object at the URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public async Task<string> DeleteAsyncAsString(string url = "", string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Delete, null, datatype, transaction).ConfigureAwait(false);
			var content = response.ResponseAsString();
			return content;
		}

		/// <summary>
		/// Deletes the specified object at the URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public async Task<T> DeleteAsync<T>(string url = "", string datatype = "json", Transaction transaction = null)
		{
			var response = await Execute(url, HttpMethod.Delete, null, datatype, transaction).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				TestInternalServerError(response);
				return default(T);
			}

			var content = response.ResponseAsString();
			return HttpResponseMessageExtensions.DeserializeHttpResponse<T>(content, datatype);
		}

		public string HttpInternalServerErrorJsonSchema =
			@"{
				'type': 'object',
				'properties': {
					'Message': {'type': 'string'},
					'ExceptionMessage': {'type': 'string'},
					'ExceptionType': {'type': 'string'},
					'StackTrace': {'type': 'string'},
					'InnerException': {'type': 'object'}
				}	
			}";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="response"></param>
		public void TestInternalServerError(HttpResponseMessage response)
		{
			//	only attempt to parse HTTP 500 errors
			if (response.StatusCode != HttpStatusCode.InternalServerError) { return; }

			JsonSchema schema = JsonSchema.Parse(HttpInternalServerErrorJsonSchema);
			var jsonResponse = response.ResponseAsString();
			JObject jsonObject = JObject.Parse(jsonResponse);
			if (!jsonObject.IsValid(schema))
			{
				//	it's not the Internal Server Error information that .NET produces
				return;
			}

			//	this should successfully deserialize
			throw new Exception(jsonResponse);
		}

		/// <summary>
		/// Configures the initial client headers. This should only be run during
		/// initialization. But, it can be used later on.
		/// </summary>
		public void ConfigureHeaders()
		{
			if (RequestHeaders != null)
			{
				foreach (var requestHeader in RequestHeaders)
				{
					var list = new List<string>() { requestHeader.Value };
					var header = new KeyValuePair<string, IEnumerable<string>>(requestHeader.Key, list);
					if (!DefaultRequestHeaders.Contains(header, RequestHeaderComparer))
					{
						DefaultRequestHeaders.Add(requestHeader.Key, requestHeader.Value);
					}
				}
			}
		}

		/// <summary>
		/// Configures the clients return datatype and sets request headers.
		/// </summary>
		/// <param name="datatype">The return datatype.</param>
		public void ConfigureRequestHeaders(string datatype = "json", HttpRequestMessage request = null)
		{
			MediaTypeWithQualityHeaderValue acceptHeader = null;

			var toUpdate = DefaultRequestHeaders;
			if (request != null)
			{
				toUpdate = request.Headers;
			}

			switch (datatype.ToLower())
			{
				case "json":
					acceptHeader = new MediaTypeWithQualityHeaderValue("application/json");
					if(!toUpdate.Accept.Contains(acceptHeader)) { toUpdate.Accept.Add(acceptHeader); }
					break;
				case "xml": case "rawxml":
					acceptHeader = new MediaTypeWithQualityHeaderValue("text/xml");
					if (!toUpdate.Accept.Contains(acceptHeader)) { toUpdate.Accept.Add(acceptHeader); }
					acceptHeader = new MediaTypeWithQualityHeaderValue("application/xml");
					if (!toUpdate.Accept.Contains(acceptHeader)) { toUpdate.Accept.Add(acceptHeader); }
					break;
			}
		}

		// Custom comparer for the Product class
		class HeaderComparer : IEqualityComparer<KeyValuePair<string,IEnumerable<string>>>
		{
			// Products are equal if their names and product numbers are equal.
			public bool Equals(KeyValuePair<string, IEnumerable<string>> x, KeyValuePair<string, IEnumerable<string>> y)
			{

				//Check whether the compared objects reference the same data.
				if (Object.ReferenceEquals(x, y)) return true;

				//Check whether any of the compared objects is null.
				if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
					return false;

				//Check whether the products' properties are equal.
				if (x.Key != y.Key) return false;
				foreach(var value in x.Value)
				{
					if (!y.Value.Contains(value)) return false;
				}

				return true;
			}

			// If Equals() returns true for a pair of objects 
			// then GetHashCode() must return the same value for these objects.

			public int GetHashCode(KeyValuePair<string, IEnumerable<string>> kvp)
			{
				//Check whether the object is null
				if (Object.ReferenceEquals(kvp, null)) return 0;

				//Calculate the hash code for the product.
				int hash = kvp.Key.GetHashCode();
				foreach(var value in kvp.Value)
				{
					hash = hash ^ value.GetHashCode();
				}
				return hash;
			}

		}

		#endregion

		#region protected methods

		/// <summary>
		/// Check if the <see cref="baseAddress" /> passed into the constructor is actually a name of
		/// of a configuration element. If it is, then let the <see cref="HttpClientSaManager" />
		/// configure the instance.
		/// </summary>
		/// <param name="baseAddress">"http://something.sa.ucsb.edu/webservices/lkjasd"</param>
		protected virtual void CheckBaseAddressAndConfigure(string baseAddress)
		{
			if (!string.IsNullOrWhiteSpace(baseAddress))
			{
				//	if the base address is actually a name, check for the name in the application config
				if (baseAddress.Contains("://") == false)
				{
					var config = HttpClientSaManager.GetConfig(baseAddress);
					if (config != null)
					{
						HttpClientSaManager.ConfigureClient(this, config);
					}
				}
				else
				{
					BaseAddress = new Uri(baseAddress);
				}
			}
		}

		/// <summary>
		/// Check the <see cref="defaultConfig" /> passed into the constructor is not null. If it isn't,
		/// then let the <see cref="HttpClientSaManager" /> configure the instance.
		/// </summary>
		/// <param name="defaultConfig">The default configuration settings.</param>
		protected virtual void CheckDefaultConfigAndConfigure(HttpClientSaConfiguration defaultConfig)
		{
			if (defaultConfig == null)
			{
				throw new ArgumentNullException(
					"defaultConfig",
					"When using a defaultConfig with an HttpClientSa object, the defaultConfig variable " +
					"must not be null. Please supply an object with the configuration or use an alternate " +
					"constructor."
				);
			}

			HttpClientSaManager.ConfigureClientWithOverrideCheck(this, defaultConfig);
		}

		/// <summary>
		/// Check if the passed in <see cref="headers"/> are null. If they aren't, then configure
		/// default headers.
		/// </summary>
		/// <param name="headers"></param>
		protected virtual void CheckHeadersAndConfigure(IDictionary<string, string> headers)
		{
			if (headers != null)
			{
				RequestHeaders = headers;
				ConfigureHeaders();
			}
		}

		/// <summary>
		/// Configure the default trace level.
		/// </summary>
		/// <param name="traceLevel">The default trace level.</param>
		protected virtual void CheckTraceLevelAndConfigure(HttpClientSaTraceLevel traceLevel)
		{
			TraceLevel = traceLevel;
		}

		/// <summary>
		/// Configure the PostDbLogged callback.
		/// </summary>
		/// <param name="postDbLogged">The callback</param>
		protected virtual void CheckPostDbLoggedAndConfigure(Action<HttpCall> postDbLogged)
		{
			PostDbLogged = postDbLogged;
		}

		#endregion

		#region internal methods

		internal HttpContent GetHttpContent(object data, string datatype)
		{
			HttpContent content = null;
			string converted = null;
			MediaTypeFormatter formatter = null;
			switch (datatype.ToLower())
			{
				case "json":
					formatter = new JsonMediaTypeFormatter();
					((JsonMediaTypeFormatter)formatter).SerializerSettings = JsonNetSerializerSettings;
					converted = Serialize(formatter, data);
					//var converted = JsonConvert.SerializeObject(data, JsonNetSerializerSettings);	// replaced by Serialize()
					content = new StringContent(converted, Encoding.UTF8, "application/json");
					break;

				case "xml":
					formatter = new XmlMediaTypeFormatter();
					converted = Serialize(formatter, data);
					content = new StringContent(converted, Encoding.UTF8, "application/xml");
					break;

				case "rawxml":
					content = new StringContent(data.ToString(), Encoding.UTF8, "application/xml");
					break;
			}

			return content;
		}

		// http://www.asp.net/web-api/overview/formats-and-model-binding/json-and-xml-serialization
		internal string Serialize<T>(MediaTypeFormatter formatter, T value)
		{
			// Create a dummy HTTP Content.
			Stream stream = new MemoryStream();
			var content = new StreamContent(stream);
			/// Serialize the object.
			formatter.WriteToStreamAsync(typeof(T), value, stream, content, null).Wait();
			// Read the serialized string.
			stream.Position = 0;
			return content.ReadAsStringAsync().Result;
		}



		public virtual async Task<HttpResponseMessage> Execute(
			string url, HttpMethod method, object data, string datatype = "json", Transaction transaction = null)
		{
			HttpRequestMessage request = new HttpRequestMessage(method, url);

			ConfigureRequestHeaders(datatype, request);

			if (IgnoreImplicitTransactions == false)
			{
				request.AddTransactionPropagationToken();
			}

			HttpResponseMessage response = null;
			Exception exception = null;
			HttpContent httpContent = null;
			DateTime requestTime = DateTime.Now;

			try
			{
				switch (method.Method)
				{
					case "PUT":
					case "POST":
						httpContent = GetHttpContent(data, datatype);
						request.Content = httpContent;
						response = await SendAsyncUcsb(request).ConfigureAwait(false);
						break;

					default:
						response = await SendAsyncUcsb(request).ConfigureAwait(false);
						break;

				}
			}
			catch (Exception e)
			{

				// If the background thread which creates an EF DB context and there is a "currently unhandled exception" on
				// the stack, then EF won't create the DB Context. So, if an exception is rethrown and this code run in the
				// finally block, then nothing gets written to the database.
				  
				// Having this in the catch block gets around this behavior. But, if no exception occurs then we still need
				// to write the info to the database; so we have to duplicate the code in the finally block.
				 
				exception = e;

				PostCallProcessing(request, response, exception, requestTime, data, datatype, PostDbLogged, PostException);
				
				throw;
			}
			finally
			{
				//	see catch block above for explanation
				if (exception == null)
				{	
					PostCallProcessing(request, response, exception, requestTime, data, datatype, PostDbLogged, PostException);
				}
			}
			return response;
		}

		/// <summary>
		/// A wrapper for <see cref="HttpClient.SendAsync(HttpRequestMessage, CancellationToken)" />. This
		/// is used internally to provide the same functionality as <see cref="DelegatingHandler" />s. Here's
		/// a stackoverflow post on <see cref="HttpClientFactory" />,
		/// https://stackoverflow.com/questions/18976042/httpclientfactory-create-vs-new-httpclient.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<HttpResponseMessage> SendAsyncUcsb(
			HttpRequestMessage request,
			CancellationToken cancellationToken = default(CancellationToken)
		)
		{
			if (this.SendAsyncMessageHandler != null)
			{
				var invoker = new HttpMessageInvoker(this.SendAsyncMessageHandler);
				return invoker.SendAsync(request, cancellationToken);
			}

			//	this next line _should_ never run; but its a fall back if needed
			return this.SendAsync(request, cancellationToken);
		}

		/// <summary>
		/// This will colect all the final information needed to write the call information to the database
		/// </summary>
		/// <param name="request">Http Request</param>
		/// <param name="response">Http Response</param>
		/// <param name="exception">If an exception occured, this is the exception</param>
		/// <param name="requestTime">Request Date time </param>
		/// <param name="postDbLogged">A callback for after the call has been logged to the database</param>
		/// <param name="data">The payload that gets encoded into the request</param>
		/// <param name="datatype">The datatype of the encoding (ie. json, etc).</param>
		/// <param name="postException">A callback for after the exception happens</param>
		public void PostCallProcessing(
			HttpRequestMessage request,
			HttpResponseMessage response,
			Exception exception,
			DateTime requestTime,
			object data,
			string datatype = "json",
			Action<HttpCall> postDbLogged = null,
			Action<HttpCall> postException = null 
		)
		{
			DateTime responseTime = DateTime.Now;
			LastHttpResponseMessage = response;

			switch (TraceLevel)
			{
				case HttpClientSaTraceLevel.Error:
					if (exception != null)
					{
						SaveCall(request, response, exception, requestTime, responseTime, data, datatype, PostDbLogged, PostException);
					}
					break;

				case HttpClientSaTraceLevel.All:
					SaveCall(request, response, exception, requestTime, responseTime, data, datatype, PostDbLogged, PostException);
					break;
			}
		}

		/// <summary>
		/// Save the call information to the trace tables. This will save the call in a background
		/// thread. So, if the application closes exits too quickly or an exception occurs nothing
		/// will be written to the database.
		/// </summary>
		/// <param name="request">Http Request</param>
		/// <param name="response">Http Response</param>
		/// <param name="exception">If an exception occured, this is the exception</param>
		/// <param name="requestDateTime">Request Date time </param>
		/// <param name="responseDateTime">Response Datetime</param>
		/// <param name="postDbLogged">A callback for after the call has been logged to the database</param>
		/// <param name="data">The payload that gets encoded into the request</param>
		/// <param name="datatype">The datatype of the encoding (ie. json, etc).</param>
		/// <param name="postException">A callback for after the exception happens</param>
		public void SaveCall(
			HttpRequestMessage request,
			HttpResponseMessage response,
			Exception exception,
			DateTime requestDateTime,
			DateTime responseDateTime,
			object data,
			string datatype = "json",
			Action<HttpCall> postDbLogged = null,
			Action<HttpCall> postException = null
		)
		{
			var call = CreateCall(request, response, requestDateTime, responseDateTime, data, datatype);
			var errors = CreateErrors(request, requestDateTime, exception);
			SaveCall(call, errors, postDbLogged, postException);
		}

		/// <summary>
		/// Save the call information to the trace tables. This will save the call in a background
		/// thread. So, if the application closes exits too quickly or an exception occurs nothing
		/// will be written to the database.
		/// </summary>
		/// <param name="httpCall">Http Call Object for Tracing</param>
		/// <param name="errors">If an exception occurred on the call, and all of its inner exceptions</param>
		/// <param name="postDbLogged">A callback for after the call has been logged to the database</param>
		/// <param name="postException">A callback for after the exception happens</param>
		public void SaveCall(
			HttpCall call,
			List<HttpError> errors,
			Action<HttpCall> postDbLogged,
			Action<HttpCall> postException
		)
		{
			var args = new SaveCallArgs() { Call = call, Errors = errors, PostDbLogged = postDbLogged, PostException = postException};
			ThreadPool.QueueUserWorkItem(SaveCall, args);
			//SaveCall(args);
		}

		/// <summary>
		/// Create the Call object which can be saved to the tracing tables. This will
		/// not save. Use SaveCall to create the object and save.
		/// </summary>
		/// <param name="request">Http Request</param>
		/// <param name="response">Http Response</param>
		/// <param name="requestDateTime">Request Date time </param>
		/// <param name="responseDateTime">Response Datetime</param>
		/// <param name="data">The payload that gets encoded into the request</param>
		/// <param name="datatype">The datatype of the encoding (ie. json, etc).</param>
		public HttpCall CreateCall(
			HttpRequestMessage request,
			HttpResponseMessage response,
			DateTime requestDateTime,
			DateTime responseDateTime,
			object data,
			string datatype = "json"
		)
		{
			HttpCall call = new HttpCall();

			// request info
			if(request.Content != null)
			{
				var newContent = GetHttpContent(data, datatype);
				call.RequestBody = newContent.ReadAsStringAsync().Result;
			}
			call.Method = request.Method.Method;
			call.Server = Environment.MachineName;
			call.RequestDate = responseDateTime;

			//	if the requestUri has the schema in it (http://) then only the request uri should
			//	be recorded. The HttpClient object will ignore the BaseAddress if the full url path
			//	is given in the requestUri.
			call.Uri = request.RequestUri.AbsoluteUri;
			
			// request header 
			string headers = string.Empty;
			int counter = 0;

			foreach (var header in request.Headers)
			{
				headers += header.Key + " = " + header.Value.ElementAt(counter) + Environment.NewLine;

			}
			call.RequestHeader = headers;

			call.ResponseDate = responseDateTime;   // this always has to be set or EF blows up because .NET DateTime Range
													// starts at 1/1/0001, and that date is father back than SQL's DateTime
													// can handle. SQL's DateTime2 can handle it, but we didn't use that.
													// Error Message: The conversion of a datetime2 data type to a datetime data type resulted in an out-of-range value
			if (response != null)
			{
				// response info 
				call.ResponseBody = response.Content.ReadAsStringAsync().Result;
				call.StatusCode = (int)response.StatusCode;

				// create header info from header collection 
				var responseheaders = string.Empty;
				foreach (var header in response.Headers)
				{
					responseheaders += header.Key + " = " + header.Value + Environment.NewLine;
				}
				// Header info from request header 
				call.ResponseHeader = responseheaders;
			}

			// time diff between call 
			call.TimeDiff = responseDateTime - requestDateTime;
			// direction of request 
			call.Direction = RequestDirection.Out;

			return call;
		}

		/// <summary>
		/// On Error call this even handler 
		/// </summary>
		/// <param name="request">request</param>
		/// <param name="sender">sender</param>
		/// <param name="e"></param>
		public List<HttpError> CreateErrors(HttpRequestMessage request, DateTime requestDateTime, Exception exception)
		{
			var errors = new List<HttpError>();

			var uri = request.RequestUri.AbsoluteUri;
			
			var currentException = exception;
			while (currentException != null)
			{
				var error = new HttpError();
				error.Uri = uri;
				error.RequestDate = requestDateTime;
				error.Message = currentException.Message;
				error.Source = currentException.Source;
				error.StackTrace = currentException.StackTrace;
				error.TargetSite = currentException.TargetSite.Name;
				error.Type = currentException.GetType().FullName;

				errors.Add(error);
				currentException = currentException.InnerException;
			}

			

			return errors;
		}

		/// <summary>
		/// Object state to be used with the callback
		/// </summary>
		private class SaveCallArgs
		{
			public HttpCall Call;
			public List<HttpError> Errors;
			public Action<HttpCall> PostDbLogged;
			public Action<HttpCall> PostException;

		}

		/// <summary>
		/// Private method to insert call log  
		/// </summary>
		/// <param name="state">Call Log object</param>
		internal static void SaveCall(object state)
		{
			try
			{
				var args = state as SaveCallArgs;
				var call = args.Call;
				var uri = new Uri(call.Uri);
				var addresses = System.Net.Dns.GetHostAddresses(uri.Host);
				if (addresses.Any()) { call.IP = addresses[0].ToString(); }

				// store in instumentation database 
				using (var db = new InstrumentationDbContext())
				{
					db.Calls.Add(call);
					db.SaveChanges();

					var errors = args.Errors;
					if (errors.Count > 0)
					{
						foreach (var error in errors)
						{
							error.CallId = call.CallId;
							db.Errors.Add(error);
						}
						db.SaveChanges();

						if (args.PostException != null)
						{
							args.PostException(call);
						}
					}

					
				}

				if (args.PostDbLogged != null)
				{
					args.PostDbLogged(call);
				}
			} catch
			{
				/*
					MAGLIO-S - 2016-03-05: After discussion with Nikhil, Aurelien, and Seth
					it was decided to swallow all exceptions that occur while
					saving on the background thread. The end user is unable
					to catch these exceptions and they cause w3wp.exe to crash.
				*/
			}
			
		}

		#endregion

	}
}
