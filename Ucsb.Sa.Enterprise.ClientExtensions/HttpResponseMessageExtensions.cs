using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
    /// <summary>
    /// Extension methods for the HttpResponseMessage object.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {

        /// <summary>
		/// Retrieves the result of the request and converts the body of the result to a string.
		/// </summary>
		/// <param name="responseTask">The async response task.</param>
		/// <returns>The body of the response as a string.</returns>
		public static string ResponseAsString(this Task<HttpResponseMessage> responseTask)
        {
            var response = responseTask.Result;
            return ResponseAsString(response);
        }

        /// <summary>
        /// Converts the result of the request and converts the body of the result to a string.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>The body of the response as a string.</returns>
        /// <exception cref="Exception">REST request failed with error code:  + response.StatusCode</exception>
        public static string ResponseAsString(this HttpResponseMessage response)
        {
            Task<string> readContent = response.Content.ReadAsStringAsync();
            return readContent.Result;
        }

        /// <summary>
        /// Deserialize a string using a particular datatype.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="result">
        /// The result from the <see cref="HttpResponseMessage"/>.
        /// Generally from <see cref="ResponseAsString(HttpResponseMessage)"/>.</param>
        /// <param name="datatype">The datatype of the content (ie. json, etc.)</param>
        /// <returns>
        /// If a successful response from the server, then deserialize to the object. Otherwise,
        /// a default object of the given type T is returned.
        /// </returns>
        public static T Deserialize<T>(this Task<HttpResponseMessage> responseTask, string datatype = "json")
        {
            var response = responseTask.Result;
            if(!response.IsSuccessStatusCode) { return default(T); }

            var content = ResponseAsString(response);
            return DeserializeHttpResponse<T>(content, datatype);
        }

		/// <summary>
		/// Deserialize a string using a particular datatype.
		/// </summary>
		/// <typeparam name="T">The type to deserialize to.</typeparam>
		/// <param name="result">
		/// The result from the <see cref="HttpResponseMessage"/>.
		/// Generally from <see cref="ResponseAsString(HttpResponseMessage)"/>.</param>
		/// <param name="datatype">The datatype of the content (ie. json, etc.)</param>
		/// <returns>
		/// If a successful response from the server, then deserialize to the object. Otherwise,
		/// a default object of the given type T is returned.
		/// </returns>
		public static T Deserialize<T>(this HttpResponseMessage response, string datatype = "json")
		{
			if (!response.IsSuccessStatusCode) { return default(T); }

			var content = ResponseAsString(response);
			return DeserializeHttpResponse<T>(content, datatype);
		}

		/// <summary>
		/// Deserialize a string using a particular datatype.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result">
		/// The result from the <see cref="HttpResponseMessage"/>.
		/// Generally from <see cref="ResponseAsString(HttpResponseMessage)"/>.</param>
		/// <param name="datatype">The datatype of the content (ie. json, etc.)</param>
		/// <returns></returns>
		public static T DeserializeHttpResponse<T>(string result, string datatype = "json")
		{
			MediaTypeFormatter formatter = null;
            T deserialized = default(T);
            switch (datatype.ToLower())
            {
                case "json":
					formatter = new JsonMediaTypeFormatter();
		            deserialized = DeserializeGeneric<T>(formatter, result);
		            //deserialized = JsonConvert.DeserializeObject<T>(result); break;
		            break;
				case "xml":
					formatter = new XmlMediaTypeFormatter();
					deserialized = DeserializeGeneric<T>(formatter, result);
					//deserialized = JsonConvert.DeserializeObject<T>(result); break;
					break;
				case "rawxml":
			        throw new ArgumentException(
						"datatype",
						"When using datatype 'rawxml', the only valid return type is 'string'. " +
						"Please use GetAsyncAsString, PutAsyncAsString, PostAsyncAsString, or DeleteAsyncAsString instead."
					);
		            break;
            }
            return deserialized;
        }

		// http://www.asp.net/web-api/overview/formats-and-model-binding/json-and-xml-serialization
		internal static T DeserializeGeneric<T>(MediaTypeFormatter formatter, string str)
		{
			// Write the serialized string to a memory stream.
			Stream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(str);
			writer.Flush();
			stream.Position = 0;
			// Deserialize to an object of type T
			return (T) formatter.ReadFromStreamAsync(typeof(T), stream, null, null).Result;
		}

	}
}
