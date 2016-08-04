using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSa"/> class.
		/// </summary>
		public HttpClientSa()
		{
			RequestHeaders = new Dictionary<string, string>();
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
			if(!string.IsNullOrWhiteSpace(baseAddress))
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
		/// Initializes a new instance of the <see cref="HttpClientSa"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers to add to every call.</param>
		public HttpClientSa(string baseAddress, IDictionary<string,string> headers) :
			this(baseAddress)
		{
			if(headers != null)
			{
				RequestHeaders = headers;
				ConfigureHeaders();
			}
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
			TraceLevel = traceLevel;
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
			PostDbLogged = postDbLogged;
		}

		#endregion

		#region public methods

		/// <summary>
		/// Gets the data object at the specified URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns>The string body of the returned result.</returns>
		public string Get(string url = "", string datatype = "json")
		{
			return GetAsyncAsString(url, datatype).Result;
		}

		/// /// <summary>
		/// Gets the data object at the specified URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns>The string body of the returned result.</returns>
		public T Get<T>(string url = "", string datatype = "json")
		{
			return GetAsync<T>(url, datatype).Result;
        }

		/// <summary>
		/// Gets the data object at the specified URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns>The string body of the returned result.</returns>
		public async Task<string> GetAsyncAsString(string url = "", string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Get, null, datatype);
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
		public async Task<T> GetAsync<T>(string url = "", string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Get, null, datatype);
			if (!response.IsSuccessStatusCode) { return default(T); }

			var content = response.ResponseAsString();
			return HttpResponseMessageExtensions.DeserializeHttpResponse<T>(content, datatype);
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
		public string Post(string url = "", object data = null, string datatype = "json")
		{
			return PostAsyncAsString(url, data, datatype).Result;
		}

		/// <summary>
		/// Posts the data object to the specified URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="data">The data to send in the body of the request.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public T Post<T>(string url = "", object data = null, string datatype = "json")
		{
			return PostAsync<T>(url, data, datatype).Result;
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
		public async Task<string> PostAsyncAsString(string url = "", object data = null, string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Post, data, datatype);
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
		public async Task<T> PostAsync<T>(string url = "", object data = null, string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Post, data, datatype);
			if (!response.IsSuccessStatusCode) { return default(T); }

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
		public string Put(string url = "", object data = null, string datatype = "json")
		{
			return PutAsyncAsString(url, data, datatype).Result;
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
		public T Put<T>(string url = "", object data = null, string datatype = "json")
		{
			return PutAsync<T>(url, data, datatype).Result;
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
		public async Task<string> PutAsyncAsString(string url = "", object data = null, string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Put, data, datatype);
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
		public async Task<T> PutAsync<T>(string url = "", object data = null, string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Put, data, datatype);
			if (!response.IsSuccessStatusCode) { return default(T); }

			var content = response.ResponseAsString();
			return HttpResponseMessageExtensions.DeserializeHttpResponse<T>(content, datatype);
		}

		/// <summary>
		/// Deletes the specified object at the URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public string Delete(string url = "", string datatype = "json")
		{
			return DeleteAsyncAsString(url, datatype).Result;
		}

		/// <summary>
		/// Deletes the specified object at the URL.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the returned value to.</typeparam>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public T Delete<T>(string url = "", string datatype = "json")
		{
			return DeleteAsync<T>(url, datatype).Result;
		}

		/// <summary>
		/// Deletes the specified object at the URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="datatype">The datatype of the expected result.</param>
		/// <returns></returns>
		public async Task<string> DeleteAsyncAsString(string url = "", string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Delete, null, datatype);
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
		public async Task<T> DeleteAsync<T>(string url = "", string datatype = "json")
		{
			var response = await Execute(url, HttpMethod.Delete, null, datatype);
			if (!response.IsSuccessStatusCode) { return default(T); }

			var content = response.ResponseAsString();
			return HttpResponseMessageExtensions.DeserializeHttpResponse<T>(content, datatype);
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
		public void ConfigureRequestHeaders(string datatype = "json")
		{
			MediaTypeWithQualityHeaderValue acceptHeader = null;
			switch (datatype.ToLower())
			{
				case "json": acceptHeader = new MediaTypeWithQualityHeaderValue("application/json"); break;
			}

			if (acceptHeader != null)
			{
				if (!DefaultRequestHeaders.Accept.Contains(acceptHeader))
				{
					DefaultRequestHeaders.Accept.Clear();
					DefaultRequestHeaders.Accept.Add(acceptHeader);
				}
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

		#region internal methods

		internal HttpContent GetHttpContent(object data, string datatype)
		{
			HttpContent content = null;
			switch(datatype.ToLower())
			{
				case "json":
					var converted = JsonConvert.SerializeObject(data, JsonNetSerializerSettings);
					content = new StringContent(converted, Encoding.UTF8, "application/json");
					break;
			}
			return content;
		}

		

        public virtual async Task<HttpResponseMessage> Execute(
			string url, HttpMethod method, object data, string datatype = "json")
		{
			ConfigureRequestHeaders(datatype);

			HttpResponseMessage response;
			HttpContent httpContent = null;
			DateTime requestTime = DateTime.Now;

            switch (method.Method)
            {
                case HttpVerbSa.Get:
					response = await base.GetAsync(url);
                    break;
                case HttpVerbSa.Put:
                    httpContent = GetHttpContent(data, datatype);
					response = await base.PutAsync(url, httpContent);
                    break;
                case HttpVerbSa.Post:
                    httpContent = GetHttpContent(data, datatype);
					response = await base.PostAsync(url, httpContent);
                    break;
                case HttpVerbSa.Delete:
					response = await base.DeleteAsync(url);
                    break;
                default:
                    throw new ArgumentException(String.Format("'{0}' is not a supported HTTP Verb", method.Method), "method");

            }
			DateTime responseTime = DateTime.Now;
			LastHttpResponseMessage = response;

			switch(TraceLevel)
			{
				case HttpClientSaTraceLevel.All:
					SaveCall(response, requestTime, responseTime, data, datatype, PostDbLogged);
					break;
			}

			return response;
		}

        /// <summary>
		/// Save the call information to the trace tables. This will save the call in a background
        /// thread. So, if the application closes exits too quickly or an exception occurs nothing
        /// will be written to the database.
		/// </summary>
		/// <param name="response">Http Response</param>
		/// <param name="requestDateTime">Request Date time </param>
		/// <param name="responseDateTime">Response Datetime</param>
        /// <param name="postDbLogged">A callback for after the call has been logged to the database</param>
        /// <param name="data">The payload that gets encoded into the request</param>
        /// <param name="datatype">The datatype of the encoding (ie. json, etc).</param>
        public void SaveCall(
            HttpResponseMessage response,
            DateTime requestDateTime,
            DateTime responseDateTime,
            object data,
            string datatype = "json",
            Action<HttpCall> postDbLogged = null
        )
        {
            var call = CreateCall(response, requestDateTime, responseDateTime, data, datatype);
            SaveCall(call, postDbLogged);
        }

        /// <summary>
		/// Save the call information to the trace tables. This will save the call in a background
        /// thread. So, if the application closes exits too quickly or an exception occurs nothing
        /// will be written to the database.
		/// </summary>
		/// <param name="httpCall">Http Call Object for Tracing</param>
		/// <param name="postDbLogged">A callback for after the call has been logged to the database</param>
        public void SaveCall(
            HttpCall call,
            Action<HttpCall> postDbLogged
        )
        {
            var args = new SaveCallArgs() { Call = call, PostDbLogged = postDbLogged };
            ThreadPool.QueueUserWorkItem(SaveCall, args);
        }

        /// <summary>
        /// Create the Call object which can be saved to the tracing tables. This will
        /// not save. Use SaveCall to create the object and save.
        /// </summary>
        /// <param name="response">Http Response</param>
        /// <param name="requestDateTime">Request Date time </param>
        /// <param name="responseDateTime">Response Datetime</param>
        /// <param name="data">The payload that gets encoded into the request</param>
        /// <param name="datatype">The datatype of the encoding (ie. json, etc).</param>
        public HttpCall CreateCall(
			HttpResponseMessage response,
            DateTime requestDateTime,
            DateTime responseDateTime,
            object data,
            string datatype = "json"
		)
		{
			HttpCall call = new HttpCall();

			// request info
            if(response.RequestMessage.Content != null)
            {
                var newContent = GetHttpContent(data, datatype);
                call.RequestBody = newContent.ReadAsStringAsync().Result;
            }
			call.Method = response.RequestMessage.Method.Method;
			call.Server = Environment.MachineName;
			call.RequestDate = responseDateTime;

            //	if the requestUri has the schema in it (http://) then only the request uri should
            //	be recorded. The HttpClient object will ignore the BaseAddress if the full url path
            //	is given in the requestUri.
            call.Uri = response.RequestMessage.RequestUri.AbsoluteUri;
			
			// request header 
			string headers = string.Empty;
			int counter = 0;

			foreach (var header in response.RequestMessage.Headers)
			{
				headers += header.Key + " = " + header.Value.ElementAt(counter) + Environment.NewLine;

			}
			call.RequestHeader = headers;

			// response info 
			call.ResponseBody = response.Content.ReadAsStringAsync().Result;
			call.ResponseDate = responseDateTime;
			call.StatusCode = (int)response.StatusCode;

			// create header info from header collection 
			var responseheaders = string.Empty;
			foreach (var header in response.Headers)
			{
				responseheaders += header.Key + " = " + header.Value + Environment.NewLine;
			}
			// Header info from request header 
			call.ResponseHeader = responseheaders;

			// time diff between call 
			call.TimeDiff = responseDateTime - requestDateTime;
			// direction of request 
			call.Direction = RequestDirection.Out;

			return call;
		}

		/// <summary>
		/// Object state to be used with the callback
		/// </summary>
		private class SaveCallArgs
		{
			public HttpCall Call;
			public Action<HttpCall> PostDbLogged;
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
                }

                if (args.PostDbLogged != null)
                {
                    args.PostDbLogged(call);
                }
            } catch
            {
                /*
                    MAGLIO-S - 2016-03-05: After discussion with Nikhil, Aruelien, and Seth
                    it was decided to swallow all exception that occur while
                    saving on the background thread. The end user is unable
                    to catch these exceptions and they cause w3wp.exe to crash.
                */
            }
			
		}

		#endregion

	}
}
