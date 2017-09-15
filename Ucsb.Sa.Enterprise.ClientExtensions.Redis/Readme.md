### Ucsb.Sa.Enterprise.ClientExtensions.Redis

A Redis implementation of of the Caching functionality
for `HttpClientSa`.

#### Installation

* [Installation Instuctions for `Ucsb.Sa.Enteprise.ClientExtensions`](https://sist-sa-ucsb.visualstudio.com/Ucsb.Sa.Enterprise/_versionControl?path=%24%2FUcsb.Sa.Enterprise%2FUcsb.Sa.Enterprise.ClientExtensions%2FDev%2FUcsb.Sa.Enterprise.ClientExtensions%2Freadme.md&version=T&_a=preview)

Using `nuget` to install the `Ucsb.Sa.Enterprise.ClientExtensions.Redis.Debug` package
will put the necessary libraries and references in place.


If you are planning on predefining `httpclient`'s in the .config file you should replace this:

```xml
<section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions" />
```

with this:
```xml
<section name="clientExtensions" type="Ucsb.Sa.Enterprise.ClientExtensions.Configuration.ClientExtensionsRedisConfigurationSection,Ucsb.Sa.Enterprise.ClientExtensions.Redis" />
```


This allows the `<clientExtensions>` section to add a `redisHost` attribute. By default, the
`HttpClientSaRedis` object will attempt to determine the correct redis host name to use based
upon the .config files `environment` value. The `redisHost` property can be used to override that:

```xml
<clientExtensions redisHost="appcaching.test.sa.ucsb.edu">
    ...
</clientExtensions>
```

#### Usage

Requirements:

You will need to have these two .config file values:

```xml
<appSettings>
    <add key="applicationName" value="{example Ucsb.Sa.Registrar.Students}" />
    <add key="environment" value="[local|int|qa|prod]" />
</appSettings>
```


Usage in code:

You can create an `HttpClientSaRedis` object directly (it extends `HttpClientSa`):

```csharp
using (var client = new HttpClientSaRedis())
{
	//  default cache timeout is 6 hours
	var response = client.GetCached<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1");
	Assert.AreEqual(1, response.userId);
	Assert.AreEqual(1, response.id);
	Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
	Assert.AreEqual("quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto", response.body);

	//  you can use a CacheItemPolicy to change the timeout
	//  NOTE:	this is an example, because this urls result was cached in the call above, this second call
	//		will pull the result from the cache and WILL NOT update the original caching policy value.
	//  NOTE:	this library isn't able to use SlidingExpirations. You can send in a SlidingExpiration policy
	//		to GetCached, but it will be converted to an AbsoluteExpiration.
	var timeoutPolicy = new CacheItemPolicy() {AbsoluteExpiration = DateTime.Now.AddHours(1)};
	response = client.GetCached<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1", policy: timeoutPolicy);

	//  both GetCached and GetCachedAsync use the same cache (key'd from the url)
	response = client.GetCachedAsync<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1").Result;
	Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
}
```

But, usually you will be extending the class:

```csharp
public class DodadProxy : HttpClientSaRedis {
    
    private HttpClientSaConfiguration __DefaultConfig =
        new HttpClientSaConfiguration() {
            Name = "Reg.Dodad",
            BaseAddress = "https://registrar.{env}.sa.ucsb.edu/webservices/dodad/"
        };

    public DodadProxy() : base(__DefaultConfig) {}

    public async Task<Dodad> Get(string param1) {
        var url = "?param1=" + param1;
        return await GetAsync<Dodad>(url: url);
    }

    public async Task<List<DodadStateTranslation>> GetDodadStateTranslations() {
        return await GetCachedAsync<DodadStateTranslation>(url: "statetranslations");
    }
}
```

If you create any instance of `HttpClientSaRedis` is will automatically update
the `HttpClientSaManager` to create new instance of `HttpClientSaRedis` instead of
`HttpClientSa`. Usage like that might look something like:

`App_Start\HttpClientSaConfig.cs`:
```csharp
public class HttpClientSaConfig {
    public void Configure() {
        // loads HttpClientSaManager to create new instances of HttpClientSaRedis
        new HttpClientSaRedis();

        using(var client = HttpClientSaManager.Get("preconfiguredClient")) {
            //  client will be of type HttpClietnSaRedis
            var result = client.GetAsString("someendpoint");
        }
    }
}
```



