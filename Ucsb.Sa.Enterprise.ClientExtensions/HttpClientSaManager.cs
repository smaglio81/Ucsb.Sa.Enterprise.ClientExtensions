using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
			GetNewClientInstance = () => new HttpClientSa();	// this is used in Get(string name, bool forceNewInstance = false)
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

		public static Func<HttpClientSa> GetNewClientInstance { get; set; }

		#endregion

		#region public methods

		/// <summary>
		/// Creates a new instance of a HttpClientSa client. This object should be disposed after usage.
		/// The new instance will use the default configuration.
		/// </summary>
		/// <returns>A new instance of <see cref="HttpClientSa" />.</returns>
		/// <exception cref="Exception">HttpClientSaManager could not find a default name for client configurations.
		/// Please ensure a default configuration and name are defined before usage.</exception>
		[Obsolete("Use Get() instead. After Singleton instances became the default creation pattern the name NewClient became incorrect." +
		          "This has been replaced by the method Get().")]
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
		[Obsolete("After Singleton instances became the default creation pattern the name NewClient became incorrect." +
				  "This has been replaced by the method Get(string name, bool forceNewInstance = false).")]
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
					client = GetNewClientInstance();
					ConfigureClient(client, config);

					_Singletons.TryAdd(name, client);
				}
			}
			else
			{
				client = GetNewClientInstance();
				ConfigureClient(client, config);
			}

			return client;
		}

		/// <summary>
		/// Configure an <see cref="HttpClientSa" /> <paramref name="client" /> with the given 
		/// <see cref="HttpClientSaConfiguration" /> <paramref name="config" /> information. This should only
		/// be used once when creating a new <see cref="HttpClientSa" /> object. This will perform an extra
		/// check before configuring the like to see an override for the "default" value is given in the config.
		/// </summary>
		/// <param name="client">The client to configure</param>
		/// <param name="config">The configuration to apply</param>
		public static void ConfigureClientWithOverrideCheck(HttpClientSa client, HttpClientSaConfiguration config)
		{
			HttpClientSaConfiguration fromConfigFile;
			if (TryGetConfig(config.Name, out fromConfigFile))
			{
				config = fromConfigFile;	//	override the default configuration
			}
			ConfigureClient(client, config);
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
				
				if (config.BaseAddress.Contains("{env}"))
				{
					//	the base address uses an {env} substitution
					client.BaseAddress = new Uri(ReplaceEnv(config.BaseAddress, client));
				} else {
					//	a normal base address
					client.BaseAddress = new Uri(config.BaseAddress);
				}
			}
			client.RequestHeaders = config.Headers;
			client.ConfigureHeaders();
			client.TraceLevel = config.TraceLevel;
			client.SerializeToCamelCase = config.SerializeToCamelCase;
			client.IgnoreImplicitTransactions = config.IgnoreImplicitTransactions;
			client.PostDbLogged = config.PostDbLogged;


			if (client.SendAsyncMessageHandler == null)
			{
				client.SendAsyncMessageHandler = new HttpClientSaRootDelegatingHandler(client);
			}

			foreach (var handlerDef in config.DelegatingHandlers)
			{
				var assembly = Assembly.Load(handlerDef.AssemblyName);
				var type = assembly.GetTypes().FirstOrDefault(i => i.Name == handlerDef.ClassName);
				if (type == null)
				{
					var message = string.Format(
						"HttpClient '{0}'s configuration has been defined with a DelegatingHandler of " +
						"class '{1}' in assembly '{2}'. The class '{1}' could not be found " +
						"in assembly with full name '{3}'. Please check the class name is correct and" +
						"that it exists within the assembly.",
						config.Name, handlerDef.ClassName, handlerDef.AssemblyName,
						assembly.FullName
					);
					throw new Exception(message);
				}

				var handlerObjInst = assembly.CreateInstance(type.FullName);
				if (handlerObjInst is DelegatingHandler)
				{
					var handlerInst = handlerObjInst as DelegatingHandler;
					handlerInst.InnerHandler = client.SendAsyncMessageHandler;
					client.SendAsyncMessageHandler = handlerInst;
				}
				else
				{
					var message = string.Format(
						"HttpClient '{0}'s configuration has been defined with a DelegatingHandler of " +
						"class '{1}' in assembly '{2}'. The instance of the class with full name '{3}' " +
						"in assembly full name '{4}' does not extend class System.Net.Http.DelegatingHandler. " +
						"DelegatingHandlers must extend this class to be used.",
						config.Name, handlerDef.ClassName, handlerDef.AssemblyName,
						handlerObjInst.GetType().FullName, assembly.FullName
					);
					throw new Exception(message);
				}
			}
		}

		/// <summary>
		/// Replaces an {env} substition within a url. It will convert strings like
		/// http://registrar.{env}.sa.ucsb.edu/webservices/students to
		/// http://registrar.dev.sa.ucsb.edu/webservices/students in the "dev" environment.
		/// </summary>
		/// <param name="url">The url the convert.</param>
		/// <param name="client">The HttpClientSa object its being converted for (this is for error reporting).</param>
		/// <returns>The converted url.</returns>
		public static string ReplaceEnv(string url, HttpClientSa client)
		{
			var environment = ConfigurationManager.AppSettings["environment"];

			// validate null
			if (string.IsNullOrWhiteSpace(environment))
			{
				throw new ArgumentException(
					"When using an HttpClientSa object with a url that contains an {env} substitution (" + url +
					") you must include a setting in appSettings/add[name=\"environment\"] for the {env} " +
					"substitution to use. This error was found when configuring " + client.GetType().FullName + "."
				);
			}

			//	validate value
			switch (environment.ToLower().Trim())
			{
				case "local":
				case "localhost":
					environment = "local";
					break;

				case "dev":
				case "development":
				case "int":
				case "integration":
					environment = "dev";
					break;

				case "qa":
				case "quality assurance":
				case "test":
					environment = "test";
					break;

				case "pilot":
				case "staging":
				case "prod":
				case "production":
					environment = "prod";
					break;

				default:
					throw new ArgumentException(
						"When using an HttpClientSa object with a url that contains an {env} substitution (" + url +
						") the environment value found in appSettings/add[name=\"environment\"] " +
						"must be either 'local', 'dev', 'test', or 'prod'. " +
						"This error was found when configuring " + client.GetType().FullName + "."
					);
			}

			//	add http to ensure new Uri doesn't throw an error
			if (url.StartsWith("http") == false) { url = "http://" + url; }
			
			//	do the {env} substitution
			if (url.Contains("{env}"))
			{
				switch (environment.ToLower())
				{
					case "prod":
						if (url.Contains(".{env}"))
						{
							url = url.Replace(".{env}", ""); // converts http://www.{env}.sa.ucsb.edu/
						} else {
							if (url.Contains("/{env}."))
							{
								url = url.Replace("{env}.", ""); // converts http://{env}.duels.lsaa.ucsb.edu/
							} else {
								url = url.Replace("{env}", ""); // converts http://nonnorm.sa.ucsb.edu/{env}/ -- shouldn't be done
							}
						}
						break;
					default:
						url = url.Replace("{env}", environment.ToLower());
						break;
				}
			}

			return url;
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

				var delegatingHandlers = new List<DelegatingHandlerDefinition>();
				foreach (DelegatingHandlerConfigurationElement handler in client.DelegatingHandlers)
				{
					var dhandler = new DelegatingHandlerDefinition()
					{
						ClassName = handler.Class,
						AssemblyName = handler.Assembly
					};
					delegatingHandlers.Add(dhandler);
				}

				var config = new HttpClientSaConfiguration()
				{
					Name = client.Name,
					BaseAddress = client.BaseAddress,
					Headers = headers,
					DelegatingHandlers = delegatingHandlers,
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
		public static bool TryGetConfig(string name, out HttpClientSaConfiguration config)
		{
			try
			{
				config = GetConfig(name);
				return true;
			}
			catch {}

			config = null;
			return false;
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

			if (config == null)
			{
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
