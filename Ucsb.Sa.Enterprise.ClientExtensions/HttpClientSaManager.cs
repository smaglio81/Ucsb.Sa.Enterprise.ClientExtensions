using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ucsb.Sa.Enterprise.ClientExtensions.Configuration;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// Handles the configuration and management of multiple clients. This can be used at application
	/// startup to register all external endpoints the application is going to use.
	/// </summary>
	public static class HttpClientSaManager
	{

		#region internal variables

		internal static readonly ConcurrentDictionary<string,HttpClientSaConfiguration> _Configs =
			new ConcurrentDictionary<string, HttpClientSaConfiguration>();

		internal static readonly ConcurrentDictionary<string, HttpClientSa> _Singletons =
			new ConcurrentDictionary<string, HttpClientSa>();

		internal static bool _ConfigLoaded = false;

		#endregion

		#region constructor

		/// <summary>
		/// Initializes the <see cref="HttpClientSaManager"/> class.
		/// </summary>
		static HttpClientSaManager()
		{
			TraceLevel = HttpClientSaTraceLevel.None;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets or sets the default name. If no default name has been set then the value will be null.
		/// </summary>
		public static string DefaultName { get; set; }

		/// <summary>
		/// Determines which calls the client will trace. This will setup all new <see cref="HttpClientSa" />'s
		/// with this trace level.
		/// </summary>
		public static HttpClientSaTraceLevel TraceLevel { get; set; }

		#endregion

		#region public methods

		/// <summary>
		/// Creates a new instance of a HttpClientSa client. This object should be disposed after usage.
		/// The new instance will use the default configuration.
		/// </summary>
		/// <returns>A new instance of <see cref="HttpClientSa" />.</returns>
		/// <exception cref="Exception">HttpClientSaManager could not find a default name for client configurations.
		/// Please ensure a default configuration and name are defined before usage.</exception>
		[Obsolete("After Singleton instances became the default creation pattern the name NewClient became incorrent." +
		          "This has been replace by the method Get().")]
		public static HttpClientSa NewClient()
		{
			return Get();
		}

		/// <summary>
		/// Creates a new instance of a HttpClientSa client. This object should be disposed after usage.
		/// The new instance will use the default configuration.
		/// </summary>
		/// <returns>A new instance of <see cref="HttpClientSa" />.</returns>
		/// <exception cref="Exception">HttpClientSaManager could not find a default name for client configurations.
		/// Please ensure a default configuration and name are defined before usage.</exception>
		public static HttpClientSa Get()
		{
			var name = GetDefaultName();
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new Exception(
					"HttpClientSaManager could not find a default name for client configurations. " +
					"Please ensure a default configuration and name are defined before usage."
				);
			}

			return Get(name);
		}

		/// <summary>
		/// Creates a new instance of a HttpClientSa client. This object should be disposed after usage.
		/// </summary>
		/// <param name="name">The unique name of the client to use.</param>
		/// <param name="forceNewInstance">Even if the client is defined as a singleton,
		/// this will for a new instance to be created.</param>
		/// <returns>A new instance of <see cref="HttpClientSa" />.</returns>
		/// <exception cref="ArgumentException">
		/// A client name must be given to lookup the HttpClientSaConfiguration. Please supply
		/// a name and try again.</exception>
		/// <exception cref="Exception">When configuration errors occur.</exception>
		[Obsolete("After Singleton instances became the default creation pattern the name NewClient became incorrent." +
				  "This has been replace by the method Get(string name, bool forceNewInstance = false).")]
		public static HttpClientSa NewClient(string name, bool forceNewInstance = false)
		{
			return Get(name, forceNewInstance);
		}

		/// <summary>
		/// Creates a new instance of a HttpClientSa client. This object should be disposed after usage.
		/// </summary>
		/// <param name="name">The unique name of the client to use.</param>
		/// <param name="forceNewInstance">Even if the client is defined as a singleton,
		/// this will for a new instance to be created.</param>
		/// <returns>A new instance of <see cref="HttpClientSa" />.</returns>
		/// <exception cref="ArgumentException">
		/// A client name must be given to lookup the HttpClientSaConfiguration. Please supply
		/// a name and try again.</exception>
		/// <exception cref="Exception">When configuration errors occur.</exception>
		public static HttpClientSa Get(string name, bool forceNewInstance = false)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(
					"A client name must be given to lookup the HttpClientSaConfiguration. Please supply " +
					"a name and try again."
				);
			}

			var config = GetConfig(name);

			//  pull from the singlton collection if the config is marked as a singleton. If it doesn't
			//  exist in the collection then create a new one.
			HttpClientSa client = null;
			if (config.IsSingleton && !forceNewInstance)
			{
				if(_Singletons.ContainsKey(name))
				{
					client = _Singletons[name];
				} else
				{
					client = new HttpClientSa();
					ConfigureClient(client, config);

					_Singletons.TryAdd(name, client);
				}
			}
			else
			{
				client = new HttpClientSa();
				ConfigureClient(client, config);
			}

			return client;
		}

		/// <summary>
		/// Configure an <see cref="HttpClientSa" /> <paramref name="client" /> with the given 
		/// <see cref="HttpClientSaConfiguration" /> <paramref name="config" /> information. This should only
		/// be used once when creating a new <see cref="HttpClientSa" /> object.
		/// </summary>
		/// <param name="client">The client to configure</param>
		/// <param name="config">The configuration to apply</param>
		public static void ConfigureClient(HttpClientSa client, HttpClientSaConfiguration config)
		{
			if (string.IsNullOrWhiteSpace(config.BaseAddress) == false)
			{
				client.BaseAddress = new Uri(config.BaseAddress);
			}
			client.RequestHeaders = config.Headers;
			client.ConfigureHeaders();
			client.TraceLevel = config.TraceLevel;
			client.SerializeToCamelCase = config.SerializeToCamelCase;
			client.IgnoreImplicitTransactions = config.IgnoreImplicitTransactions;
			client.PostDbLogged = config.PostDbLogged;
		}

		/// <summary>
		/// Force the manager to load configuration values from the application .config file.
		/// </summary>
		public static void LoadFromConfig()
		{
			var section = ClientExtensionsConfigurationSection.Configuration;
			var httpClients = section.HttpClients;
			
			//	set the default trace level (if supplied)
			if (httpClients.TraceLevel != HttpClientSaTraceLevel.Undefined)
			{
				TraceLevel = httpClients.TraceLevel;
			}

			//	load the defined client endpoints
			foreach (HttpClientConfigurationElement client in httpClients)
			{
				var headers = new Dictionary<string,string>();
				foreach(HeaderConfigurationElement header in client.Headers)
				{
					headers.Add(header.Name, header.Value);
				}

				var config = new HttpClientSaConfiguration()
				{
					Name = client.Name,
					BaseAddress = client.BaseAddress,
					Headers = headers,
					IsSingleton = client.IsSingleton,
					SerializeToCamelCase = client.SerializeToCamelCase
				};

				if(client.TraceLevel != HttpClientSaTraceLevel.Undefined)
				{
					config.TraceLevel = client.TraceLevel;
				}

				Add(config);
			}

			//	set default name if available (verify the name exists)
			if(!string.IsNullOrWhiteSpace(httpClients.DefaultName))
			{
				DefaultName = httpClients.DefaultName;
			}

			_ConfigLoaded = true;
		}

		/// <summary>
		/// Gets the <see cref="HttpClientSaConfiguration" /> for the given name.
		/// </summary>
		/// <param name="name">The name of the configuration element.</param>
		/// <returns>The <see cref="HttpClientSaConfiguration" /> element. Null, if not found.</returns>
		public static HttpClientSaConfiguration GetConfig(string name)
		{

			//	assume the configs are in the .config file if the collection is empty
			var config = GetConfigFromCollection(name);
			if (config == null)
			{
				try
				{
					if (_ConfigLoaded == false)
					{
						LoadFromConfig();
					}
				}
				catch (Exception e)
				{
					var message = string.Format(
						"No configurations were defined for name \"{0}\". An attempt was made to load " +
						"configurations from the applications .config file, but an exception occurred when " +
						"attempting to load the configuration section: {1}",
						name,
						e.Message
					);
					throw new Exception(message, e);
				}
			}

			config = GetConfigFromCollection(name);
			if (config == null)
			{
				var message = string.Format(
					"No configurations were defined for name \"{0}\". Please use Add(string name, " +
					"string baseAddress, IDictionary<string, string> headers) to add a client " +
					"definition.",
					name
				);
				throw new Exception(message);
			}

			return config;
		}

		/// <summary>
		/// Gets the <see cref="HttpClientSaConfiguration" /> for the given name.
		/// </summary>
		/// <param name="name">The name of the configuration element.</param>
		/// <returns>The <see cref="HttpClientSaConfiguration" /> element. Null, if not found.</returns>
		internal static HttpClientSaConfiguration GetConfigFromCollection(string name)
		{
			HttpClientSaConfiguration config;
			_Configs.TryGetValue(name, out config);
			return config;
		}

		/// <summary>
		/// Adds the specified client endpoint.
		/// </summary>
		/// <param name="name">The unique name.</param>
		/// <param name="baseAddress">The base address for the client.</param>
		/// <param name="headers">The default headers for the client.</param>
		/// <param name="postDbLogged">A callback to be used after a traced call is logged.</param>
		/// <returns>The new configuration element in the manager.</returns>
		public static HttpClientSaConfiguration Add(
			string name,
			string baseAddress = "",
			IDictionary<string,string> headers = null,
			Action<HttpCall> postDbLogged = null
		) {
			ValidateName(name);
			var config = new HttpClientSaConfiguration()
			{
				Name = name, BaseAddress = baseAddress, Headers = headers, PostDbLogged = postDbLogged
			};
			return Add(config);
		}

		/// <summary>
		/// Adds the specified configuration to the manager.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <returns>The new configuration element in the manager.</returns>
		public static HttpClientSaConfiguration Add(HttpClientSaConfiguration config)
		{
			ValidateConfig(config);
			if (DefaultName == null) { DefaultName = config.Name; }
			_Configs.TryAdd(config.Name, config);
			return config;
		}

		/// <summary>
		/// Updates the specified client configuration.
		/// </summary>
		/// <param name="name">The unique name.</param>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers.</param>
		/// <param name="postDbLogged">A callback to be used after a traced call is logged.</param>
		/// <returns>The updated configuration element in the manager</returns>
		public static HttpClientSaConfiguration Update(
			string name,
			string baseAddress = "",
			IDictionary<string, string> headers = null,
			Action<HttpCall> postDbLogged = null
		) {
			ValidateName(name);
			var config = new HttpClientSaConfiguration()
			{
				Name = name,
				BaseAddress = baseAddress,
				Headers = headers,
				PostDbLogged = postDbLogged
			};
			return Update(config);
		}

		/// <summary>
		/// Updates the specified client configuration.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <returns>The updated configuration element in the manager</returns>
		public static HttpClientSaConfiguration Update(HttpClientSaConfiguration config)
		{
			ValidateConfig(config);

			HttpClientSaConfiguration found;
			if(_Configs.TryGetValue(config.Name, out found))
			{
				_Configs.TryUpdate(config.Name, config, found);
				HttpClientSa client;
				_Singletons.TryRemove(config.Name, out client);
			}

			return config;
		}

		/// <summary>
		/// Removes the client configuration from the manager.
		/// </summary>
		/// <param name="name">The unique name.</param>
		/// <returns>The client configuration that was removed.</returns>
		public static HttpClientSaConfiguration Remove(string name)
		{
			ValidateName(name);
			HttpClientSaConfiguration removed;
			_Configs.TryRemove(name, out removed);
			HttpClientSa client;
			_Singletons.TryRemove(name, out client);
			return removed;
		}

		/// <summary>
		/// Removes the client configuration from the manager.
		/// </summary>
		/// <param name="config">The configuration.</param>
		/// <returns>The client configuration that was removed.</returns>
		public static HttpClientSaConfiguration Remove(HttpClientSaConfiguration config)
		{
			ValidateConfig(config);
			return Remove(config.Name);
		}

		#endregion

		#region internal methods

		internal static void ValidateName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				var message = string.Format("The name to be added the the HttpClientSaManager collection must " +
											"be valid and unique. (name = \"{0}\")", name);
				throw new ArgumentException(message);
			}
		}

		internal static void ValidateConfig(HttpClientSaConfiguration config)
		{
			if(string.IsNullOrWhiteSpace(config.Name))
			{
				var message = string.Format("The HttpClientSaConfiguration argument, config, must contain a unique " +
											"name to be added the the HttpClientSaManager collection of " +
											"configurations. (config.Name = \"{0}\")", config.Name);
				throw new ArgumentException(message);
			}
		}

		/// <summary>
		/// Gets the default <see cref="HttpClientSaConfiguration" /> for the given name.
		/// </summary>
		/// <returns>The default <see cref="HttpClientSaConfiguration" /> element. Null, if not found.</returns>
		internal static HttpClientSaConfiguration GetDefault()
		{
			var name = GetDefaultName();
			if (name == null) return null;
			return GetConfig(name);
		}

		internal static string GetDefaultName()
		{
			if (DefaultName == null)
			{
				try { LoadFromConfig(); }
				catch { /* swallow any errors */ }
			}

			if (DefaultName == null) return null;
			return DefaultName;
		}

		#endregion

	}
}
