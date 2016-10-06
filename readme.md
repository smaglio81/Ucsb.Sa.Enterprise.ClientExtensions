## Ucsb.Sa.Enterprise.ClientExtensions

Before using this project I would suggest checking these out:
    Microsoft.Rest.ClientRuntime: [https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime/](https://www.nuget.org/packages/Microsoft.Rest.ClientRuntime/)
    System.Net.Http.Formatting: [https://msdn.microsoft.com/en-us/library/system.net.http.formatting(v=vs.118).aspx](https://msdn.microsoft.com/en-us/library/system.net.http.formatting(v=vs.118).aspx)

Adds some functionality around HttpClient:

* Basic parsing of json to objects (Newtonsoft.JSON)
* Outgoing call tracing
* Response caching
* Implicit MS DTC Transaction Passing


### Client Configurations

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


