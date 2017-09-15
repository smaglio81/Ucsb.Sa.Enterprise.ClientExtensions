using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Ucsb.Sa.Enterprise.ClientExtensions.Configuration;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Redis
{
	public class HttpClientSaRedis : HttpClientSa
	{

		#region variables

		private static readonly TimeSpan __DefaultCacheTimeout = new TimeSpan(6, 0, 0);

		private readonly string __KeyPrefix;

		private readonly string __KeyPrefixApplicationNameMissing =
			"HttpClientSaRedis requires that the <appSettings> settings of the applications .config " +
			"file, needs to have an entry for 'applicationName' " +
			"(<appSettings><add key=\"application\" value=\"{name}\" /></appSettings>).";

		/// <summary>
		/// Internal cache for lookup tables.
		/// </summary>
		internal IDatabase	__RedisDb		= null;
		internal object		__RedisDbLock	= new object();

		#endregion

		#region static constructor

		static HttpClientSaRedis()
		{
			HttpClientSaManager.GetNewClientInstance = () => new HttpClientSaRedis();
		}

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSaRedis"/> class.
		/// </summary>
		public HttpClientSaRedis() : base()
		{
			string applicationName = null;
			try
			{
				applicationName = ConfigurationManager.AppSettings["applicationName"];

				if (applicationName == null)
				{
					throw new Exception(
						__KeyPrefixApplicationNameMissing +
						"This error was found when configuring " + this.GetType().FullName + "."
					);
				}
			}
			catch (Exception e)
			{
				throw new Exception(
					__KeyPrefixApplicationNameMissing +
					"This error was found when configuring " + this.GetType().FullName + ".",
					e
				);
			}

			__KeyPrefix = applicationName + ":" + GetType().FullName + ":";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSaRedis"/> class.
		/// </summary>
		/// <param name="baseAddress">
		/// The base address. This value can be overloaded to be the name of an endpoint defined
		/// in the <see cref="HttpClientSaManager" />.
		/// </param>
		public HttpClientSaRedis(string baseAddress) : this()
		{
			base.CheckBaseAddressAndConfigure(baseAddress);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSaRedis" /> class. This configuration
		/// can use a url which uses the {env} environment substitution. If this substitution is used
		/// then an appSettings/add[name="environment"] value must be supplied.
		/// </summary>
		/// <param name="defaultConfig">
		/// A default configuration for the client which can use the {env} substitution. If this
		/// substitution is used then an appSettings/add[name="environment"] value must be supplied.
		/// </param>
		public HttpClientSaRedis(HttpClientSaConfiguration defaultConfig) : this()
		{
			base.CheckDefaultConfigAndConfigure(defaultConfig);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSaRedis"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers to add to every call.</param>
		public HttpClientSaRedis(string baseAddress, IDictionary<string, string> headers) :
			this(baseAddress)
		{
			base.CheckHeadersAndConfigure(headers);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSaRedis"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers to add to every call.</param>
		/// <param name="traceLevel">The level of tracing.</param>
		public HttpClientSaRedis(string baseAddress, IDictionary<string, string> headers, HttpClientSaTraceLevel traceLevel) :
			this(baseAddress, headers)
		{
			base.CheckTraceLevelAndConfigure(traceLevel);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpClientSaRedis"/> class.
		/// </summary>
		/// <param name="baseAddress">The base address.</param>
		/// <param name="headers">The default headers to add to every call.</param>
		/// <param name="traceLevel">The level of tracing.</param>
		/// <param name="postDbLogged">A call back to use if the call is logged to the db.</param>
		public HttpClientSaRedis(
			string baseAddress,
			IDictionary<string, string> headers,
			HttpClientSaTraceLevel traceLevel,
			Action<HttpCall> postDbLogged
		) :
			this(baseAddress, headers, traceLevel)
		{
			base.CheckPostDbLoggedAndConfigure(postDbLogged);
		}

		#endregion

		#region properties

		/// <summary>
		/// The default timespan used with cache timeout's <see cref="AbsoluteTime" /> object.
		/// </summary>
		protected TimeSpan DefaultCacheTimeout { get; set; }

		/// <summary>
		/// Internal cache for lookup tables.
		/// </summary>
		protected IDatabase Cache
		{
			get
			{
				if (__RedisDb == null)
				{
					lock (__RedisDbLock)
					{
						if (__RedisDb == null)
						{
							var redis = ConnectionMultiplexer.Connect(GetHost());
							__RedisDb = redis.GetDatabase();
						}
					}
				}

				return __RedisDb;
			}
		}

		#endregion

		#region public methods

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
		public override T GetCached<T>(string url, string datatype = "json", CacheItemPolicy policy = null)
		{
			var cacheKey = GetFullCacheKey(url);

			T result = default(T);

			byte[] cachedValue = Cache.StringGet(cacheKey);
			if (cachedValue != null)
			{
				result = (T)RedisJsonSerializer.Deserialize(cachedValue);
			}
			else
			{
				result = Get<T>(url: url, datatype: datatype);
				var serialized = RedisJsonSerializer.Serialize(result);

				TimeSpan timeout = GetTimeout(policy);
				Cache.StringSet(cacheKey, serialized, timeout);
			}

			return result;
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
		public override async Task<T> GetCachedAsync<T>(string url, string datatype = "json", CacheItemPolicy policy = null)
		{
			var cacheKey = GetFullCacheKey(url);

			T result = default(T);

			byte[] cachedValue = Cache.StringGet(cacheKey);
			if (cachedValue != null)
			{
				result = (T)RedisJsonSerializer.Deserialize(cachedValue);
			}
			else
			{
				result = await GetAsync<T>(url: url, datatype: datatype);
				var serialized = RedisJsonSerializer.Serialize(result);

				TimeSpan timeout = GetTimeout(policy);
				Cache.StringSet(cacheKey, serialized, timeout);
			}

			return result;
		}

		#endregion



		#region protected methods

		#endregion

		#region private methods

		/// <summary>
		/// Determines the redis host name (dns name).
		/// </summary>
		public string GetHost()
		{
			string host = string.Empty;

			if (ClientExtensionsRedisConfigurationSection.ConfigurationSectionExists)
			{
				var config = ClientExtensionsRedisConfigurationSection.Configuration;
				if (string.IsNullOrWhiteSpace(config.RedisHost) == false)
				{
					host = config.RedisHost;
				}
			}

			if (host == string.Empty)
			{
				var environment = ConfigurationManager.AppSettings["environment"];

				if (string.IsNullOrWhiteSpace(environment))
				{
					throw new Exception(
						"HttpClientSaRedis could not determine the default host name (appcaching.{env}.sa.ucsb.edu) " +
						"to connect to the redis server. The {env} value is determined by environment appSettings found in " +
						"applications config file. You can set this in config section <appSettings><add key=\"environment\" " +
						"value=\"[dev|test|prod]\" />. " +
						"This error was found when configuring " + this.GetType().FullName + "."
					);
				}

				//	validate value
				switch (environment.ToLower().Trim())
				{
					case "local":
					case "localhost":
					case "dev":
					case "development":
					case "int":
					case "integration":
						host = "appcaching.dev.sa.ucsb.edu";
						break;

					case "qa":
					case "quality assurance":
					case "test":
						host = "appcaching.test.sa.ucsb.edu";
						break;

					case "pilot":
					case "staging":
					case "prod":
					case "production":
						host = "appcaching.sa.ucsb.edu";
						break;

					default:
						throw new ArgumentException(
							"CachedRedisInfoRepository could not determine the environment from the name '" + environment.ToLower() + "' " +
							"in the applications config file. Possible values are 'dev', 'test', or 'prod'. " +
							"You can set this in config section <appSettings><add key=\"environment\" value=\"[dev|test|prod]\" />. " +
							"This error was found when configuring " + this.GetType().FullName + "."
						);
				}
			}

			return host;
		}

		/// <summary>
		/// Creates a full key for the cache (which should be unique).
		/// </summary>
		/// <param name="key">The key provided by the client.</param>
		/// <returns>The full key with prefix.</returns>
		private string GetFullCacheKey(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentException("The key given to method GetCachedValue must have a value.", "key");

			key = key.Replace(":", string.Empty);

			return __KeyPrefix + key;
		}

		private TimeSpan GetTimeout(CacheItemPolicy policy)
		{
			TimeSpan timeout = __DefaultCacheTimeout;
			if (policy != null)
			{
				if (policy.SlidingExpiration != TimeSpan.Zero)
				{
					timeout = policy.SlidingExpiration;
				}
				if (policy.AbsoluteExpiration != DateTimeOffset.Now)
				{
					timeout = policy.AbsoluteExpiration.LocalDateTime - DateTime.Now;
				}
			}
			return timeout;
		}

		#endregion

	}
}
