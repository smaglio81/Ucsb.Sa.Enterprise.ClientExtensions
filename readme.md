## Ucsb.Sa.Enterprise.ClientExtensions

Before using this project I would suggest checking these out:
* Microsoft.Rest.ClientRuntime: [https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime/](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime/)
* System.Net.Http.Formatting: [https://msdn.microsoft.com/en-us/library/system.net.http.formatting(v=vs.118).aspx](https://msdn.microsoft.com/en-us/library/system.net.http.formatting(v=vs.118).aspx)

Adds some functionality around HttpClient:

* Client Configuration in .config and code
    * \{env} keyword support
    * Basic Authorization Header Base64 Encoding Support
* Basic parsing of json to objects (Newtonsoft.JSON)
* Outgoing call tracing
* Response caching
* Implicit MS DTC Transaction Passing
* Delegating Handlers


#### Installation

Using `nuget` to install the `Ucsb.Sa.Enterprise.ClientExtensions.Debug` package
will put the necessary libraries and references in place.


If you are planning on predefining `httpclient`'s in the .config file you should add this
`<configSections>` declaration:

```xml
<section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions" />
```

This allows the `<clientExtensions>` section to be added. Here's an example:

```xml
<configSections>
    <section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions" />
</configSections>

<appSettings>
    <add key="applicationName" value="{example: Ucsb.Sa.Registrar.Courses}" />
    <add key="environment" value="[local|dev|qa|prod]" />
</appSettings>
 
<connectionStrings>
    <add name="Instrumentation" connectionString="Initial Catalog=Instrumentation;Data Source=instrumentation.sql.{env}.sa.ucsb.edu,2433;Integrated Security=SSPI;" />
</connectionStrings>

<clientExtensions>
    <httpClients default="client1" traceLevel="ALL|[NONE]">
        <httpClient name="client1" baseAddress="http://jsonplaceholder.typicode.com" traceLevel="ALL|[NONE]" singleton="[true]|false">
            <header name="h1" value="v1" />
        </httpClient>
    </httpClients>
</clientExtensions>
```



### Client Configurations in .config

Client configurations can be defined in code or within the config file. The purpose of the configuration is to reduce the
amount of redudant configuration information within the source code. Header values can also be predefined withitn the config
file.

```xml
<configSections>
	<section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions" />
</configSections>

<clientExtensions>
	<httpClients>
		<httpClient name="p1" baseAddress="http://jsonplaceholder.typicode.com" traceLevel="All|None" />
		<httpClient name="h1">
			<header name="n1" value="v1" />
		</httpClient>
	</httpClients>
</clientExtensions>
```

This can be used with:
```csharp
// allows for a singleton object to be created and maintained by HttpClientSaManager
using (var client = HttpClientSaManager.Get("p1"))
{
	JsonPlaceholder response = client.GetAsync<JsonPlaceholder>("posts/100").Result;
}

// this can be used to create an instance for one time usage, but it's suggested
// to use a singleton throughtout the lifetime of your application
// http://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed
using (var client = new HttpClientSa("p1"))
{
	JsonPlaceholder response = client.GetAsync<JsonPlaceholder>("posts/100").Result;
}

// or

using (var client = HttpClientSaManager.Get("p1", forceNewInstance: true))
{
	JsonPlaceholder response = client.GetAsync<JsonPlaceholder>("posts/100").Result;
}
```

Clients can also be configured within code:
```csharp
HttpClientSaManager.Remove("placeholder");
HttpClientSaManager.Add("placeholder", "http://jsonplaceholder.typicode.com/posts/100");

using(var client = HttpClientSaManager.Get("placeholder"))
{
	var response = client.Get<JsonPlaceholder>();
}
```


### Client Configuration in Code

You can also setup a default client configuration in your code. This default configuration can be
overridden by values in the .config file (if needed). Basically, the idea is that you can
create a default configuration in your code. And, if you ever need to debug it by setting
the baseAddress to something else, then you can add a .config override.

```csharp
public class DefaultConfigTestProxy: HttpClientSa
{

	public static HttpClientSaConfiguration DefaultConfig = new HttpClientSaConfiguration()
	{
		Name = "DefaultConfigTest",
		BaseAddress = "http://your.{env}.com/webservices/students/",
		Headers = new Dictionary<string, string>() { { "Authorization", "Basic encodedString" } }
	};

	public DefaultConfigTestProxy() : base(DefaultConfig) {}
}
```


### \{env\} keyword support

In your baseAddresses you can use the \{env} keyword so that default configurations will "automatically"
use the correct address for the given environment.

To use the \{env} keyword, you **have** to add an `appSettings\add[name='environment']` value
to your .config. Like so:

```xml
<appSettings>
	<add key="environment" value="local" /> <!-- local, dev, test, prod -->
</appSettings>
```

Then this will work:
```csharp
public class DefaultConfigTestProxy: HttpClientSa
{

	public static HttpClientSaConfiguration DefaultConfig = new HttpClientSaConfiguration()
	{
		Name = "DefaultConfigTest",
		BaseAddress = "http://your.{env}.com/webservices/students/",
		Headers = new Dictionary<string, string>() { { "Authorization", "Basic encodedString" } }
	};

	public DefaultConfigTestProxy() : base(DefaultConfig) {}
}
```


### Basic Authorization Header Base64 Encoding Support

When using a [Basic Authorization](https://en.wikipedia.org/wiki/Basic_access_authentication) header
you can setup the username & password and the .config configuration. Like so:

```xml
<clientExtensions>
	<httpClients>
		<httpClient name="basicAuth" baseAddress="http://jsonplaceholder.typicode.com/">
			<header name="Authorization" username="asdf" password="1234" />
		</httpClient>
	</httpClients>
</clientExtensions>
```



### Tracing

If the database is setup using sc_Create_Db_Objects.sql, tracing can be turn on for outgoing calls.
The tracing will record both the request and response.

```xml
<configSections>
	<section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions" />
</configSections>

<clientExtensions>
	<httpClients>
	  <httpClient name="p1" baseAddress="http://jsonplaceholder.typicode.com" traceLevel="All|None" />
	</httpClients>
</clientExtensions>
```

Or within code:
```csharp
HttpClientSaManager.Add("placeholder", "http://jsonplaceholder.typicode.com/posts/100");

var config = HttpClientSaManager.GetConfig("placeholder");
config.TraceLevel = HttpClientSaTraceLevel.All;

using(var client = HttpClientSaManager.Get("placeholder"))
{
	client.TraceLevel = HttpClientSaTraceLevel.None;
	var response = client.Get<JsonPlaceholder>();
}
```


### Response Caching

(note: there is a `Ucsb.Sa.Enterprise.ClientExtensions.Redis` package to give Redis caching across multiple servers.) 


Sometimes you have calls which essentially load lookup tables. These tables are fine to cache
for a limited amount of time, without a need for them to always be insync with the original
datasource.

This can be accomplished with:

```csharp
HttpClientSaManager.Add("placeholder", "http://jsonplaceholder.typicode.com/posts");

using(var client = HttpClientSaManager.Get("placeholder"))
{
	var response = client.GetCached<List<JsonPlaceholder>>();
}
```

Or async ...

```csharp
HttpClientSaManager.Add("placeholder", "http://jsonplaceholder.typicode.com/posts");

using(var client = HttpClientSaManager.Get("placeholder"))
{
	var response = await client.GetCachedAsync<List<JsonPlaceholder>>();
}
```

The default eviction time on the cache is 6 hours, but can be overridden with the parameter ```policy```.



### Implicit MS DTC Transaction Passing

This adds Transaction Id passing for MS DTC transaction outlined in this article
[https://code.msdn.microsoft.com/Distributed-Transactions-c7e0a8c2](https://code.msdn.microsoft.com/Distributed-Transactions-c7e0a8c2).

To pass across a MS DTC Transaction Id, the code must be wrapped in a ```TransactionScope```:
```csharp
using (var scope = new TransactionScope())
{
	using (var client = HttpClientSaManager.Get("p1"))
	{
		var result = client.Get();	// will implicitly add transaction id
		Assert.IsTrue(!string.IsNullOrEmpty(result));
	}

	scope.Complete();
}
```

To use the transaction on the other side, you will need to implement the
```ActionFilterAttribute``` described in the article.

You can mark a ```HttpClientSa``` to ignore transactions using the
```IgnoreImplicitTransactions``` attribute on the class or
```HttpClientSaConfiguration```  class within the Manager. The default value
of ```false``` will allow for transactions to be implicitly added.

```csharp
using (var scope = new TransactionScope())
{
	using (var client = HttpClientSaManager.Get("p1"))
	{
		client.IgnoreImplicitTransactions = true;
		var result = client.Get();	// transaction will be ignored
		Assert.IsTrue(!string.IsNullOrEmpty(result));
	}

	...

	scope.Complete();
}
```




### Delegating Handlers

Microsoft's implementation has an [HttpClientFactory](https://msdn.microsoft.com/en-us/library/system.net.http.httpclientfactory%28v=vs.118%29.aspx?f=255&MSPPError=-2147217396)
which allows for interceptors to be placed at a very low level in the call stack. They intercept the
request message right before it's sent, and right after the response is recieved. It's explained
better in this stackoverflow post: [HttpClientFactory.Create vs new HttpClient](https://msdn.microsoft.com/en-us/library/system.net.http.httpclientfactory%28v=vs.118%29.aspx?f=255&MSPPError=-2147217396).

To duplicate this feature, `HttpClientConfiguration` has been updated to take in type information
about `DelegatingHandlers` and use them in the same way `HttpClientFactory` does.

In code:
``` csharp
public class DefaultConfigTestProxy: HttpClientSa, IDefaultConfigTestService
{

	public static HttpClientSaConfiguration DefaultConfig = new HttpClientSaConfiguration()
	{
		Name = "DefaultConfigTest",
		BaseAddress = "http://registrar.{env}.sa.ucsb.edu/webservices/students/",
		Headers = new Dictionary<string, string>() { { "Authorization", "Basic encodedString" } },
		DelegatingHandlers = new List<DelegatingHandlerDefinition>()
		{
			new DelegatingHandlerDefinition() { ClassName = "DummyHandler", AssemblyName = "Ucsb.Sa.Enterprise.ClientExtensions.Tests" }
		}
	};

	public DefaultConfigTestProxy() : base(DefaultConfig) {}

	...
}
```

In configuration:
``` xml
<clientExtensions>
	<httpClients>
		<httpClient name="p1-delegatinghandler" baseAddress="http://jsonplaceholder.typicode.com/" traceLevel="All" serializeToCamelCase="false">
			<delegatingHandlers>
				<handler class="DummyHandler" assembly="Ucsb.Sa.Enterprise.ClientExtensions.Tests" />
			</delegatingHandlers>
		</httpClient>
	</httpClients>
</clientExtensions>
```
