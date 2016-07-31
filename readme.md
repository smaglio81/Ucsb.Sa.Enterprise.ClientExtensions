## Ucsb.Sa.Enterprise.ClientExtensions

This adds fucntionality for management multiple client connections within
a config file. It also adds tracing for outgoing calls.

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
using (var client = new HttpClientSa("p1"))
{
	JsonPlaceholder response = client.GetAsync<JsonPlaceholder>("posts/100").Result;
}

or

// allows for a singleton object to be created and maintained by HttpClientSaManager
using (var client = HttpClientSaManager.NewClient("p1"))
{
	JsonPlaceholder response = client.GetAsync<JsonPlaceholder>("posts/100").Result;
}
```

Clients can also be configured within code:
```csharp
HttpClientSaManager.Remove("placeholder");
HttpClientSaManager.Add("placeholder", "http://jsonplaceholder.typicode.com/posts/100");

using(var client = HttpClientSaManager.NewClient("placeholder"))
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

using(var client = HttpClientSaManager.NewClient("placeholder"))
{
	client.TraceLevel = HttpClientSaTraceLevel.None;
	var response = client.Get<JsonPlaceholder>();
}
```
