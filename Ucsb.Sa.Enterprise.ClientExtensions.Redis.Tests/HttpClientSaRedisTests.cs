using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Redis.Tests
{
	[TestClass]
	public class HttpClientSaRedisTests
	{
		[TestMethod]
		public void Get()
		{
			using (var client = new HttpClientSaRedis())
			{
				var response = client.Get("http://jsonplaceholder.typicode.com/posts/1");
				Assert.AreEqual(
					"{\n  \"userId\": 1,\n  \"id\": 1," +
					"\n  \"title\": \"sunt aut facere repellat provident occaecati excepturi optio reprehenderit\"," +
					"\n  \"body\": \"quia et suscipit\\nsuscipit recusandae consequuntur expedita et cum\\nreprehenderit molestiae ut ut quas totam\\nnostrum rerum est autem sunt rem eveniet architecto\"\n}",
					response
				);
			}

			//	Demo code (not really part of the unit test)
			using (var client = new HttpClientSaRedis())
			{
				// default cache timeout is 6 hours
				var response = client.GetCached<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1");
				Assert.AreEqual(1, response.userId);
				Assert.AreEqual(1, response.id);
				Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
				Assert.AreEqual("quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto", response.body);

				//	you can use a CacheItemPolicy to change the timeout
				//	NOTE:	this is an example, because this urls result was cached in the call above, this second call
				//			will pull the result from the cache and WILL NOT update the original caching policy value.
				//	NOTE 2:	this library isn't able to use SlidingExpirations. You can send in a SlidingExpiration policy
				//			to GetCached, but it will be converted to an AbsoluteExpiration.
				var timeoutPolicy = new CacheItemPolicy() {AbsoluteExpiration = DateTime.Now.AddHours(1)};
				response = client.GetCached<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1", policy: timeoutPolicy);

				//	both GetCached and GetCachedAsync use the same cache (key'd from the url)
				response = client.GetCachedAsync<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1").Result;
				Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
			}
		}

		[TestMethod]
		public void GetCached()
		{
			using (var client = new HttpClientSaRedis())
			{
				var warmup = client.Get("http://jsonplaceholder.typicode.com/posts/1"); // prime client, but don't care about result

				var uncached = new Stopwatch();
				try
				{
					uncached.Start();

					var response = client.GetCached<List<JsonPlaceholder>>("http://jsonplaceholder.typicode.com/posts");

					Assert.IsTrue(response[0] is JsonPlaceholder);
				}
				finally
				{
					uncached.Stop();
					Console.WriteLine("uncached: " + uncached.ElapsedMilliseconds);
				}

				var cached = new Stopwatch();
				try
				{
					cached.Start();

					var response = client.GetCached<List<JsonPlaceholder>>("http://jsonplaceholder.typicode.com/posts");

					Assert.IsTrue(response[0] is JsonPlaceholder);
				}
				finally
				{
					cached.Stop();
					Console.WriteLine("cached: " + cached.ElapsedMilliseconds);
				}

				Assert.IsTrue(uncached.ElapsedTicks > cached.ElapsedTicks);
			}
		}

		//[TestMethod]
		//public void GetTransaction()
		//{
		//	//	DEVELOPER: you need mvcextensions.local.sa.ucsb.edu setup from the MvcExtensions project to use this one.
		//	using (var scope = new TransactionScope())
		//	{
		//		using (var client = new HttpClientSa("http://mvcextensions.local.sa.ucsb.edu/api/transaction"))
		//		{
		//			var result = client.Get();	// transaction will be added
		//			Assert.IsTrue(!string.IsNullOrEmpty(result));
		//		}

		//		//scope.Complete(); // rollsback the transaction
		//	}
		//}

		[TestMethod]
		public void GetCachedAsync()
		{
			using (var client = new HttpClientSaRedis())
			{
				var warmup = client.Get("http://jsonplaceholder.typicode.com/posts/1"); // prime client, but don't care about result

				var uncached = new Stopwatch();
				try
				{
					uncached.Start();

					var task = client.GetCachedAsync<List<JsonPlaceholder>>("http://jsonplaceholder.typicode.com/posts");

					Assert.IsTrue(task.Result[0] is JsonPlaceholder);
				}
				finally
				{
					uncached.Stop();
					Console.WriteLine("uncached: " + uncached.ElapsedMilliseconds);
				}

				var cached = new Stopwatch();
				try
				{
					cached.Start();

					var task = client.GetCachedAsync<List<JsonPlaceholder>>("http://jsonplaceholder.typicode.com/posts");

					Assert.IsTrue(task.Result[0] is JsonPlaceholder);
				}
				finally
				{
					cached.Stop();
				}

				Assert.IsTrue(uncached.ElapsedTicks > cached.ElapsedTicks);
				Console.WriteLine("cached: " + cached.ElapsedMilliseconds);
			}
		}

		[TestMethod]
		public void GetTyped()
		{
			using (var client = new HttpClientSaRedis())
			{
				var response = client.Get<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1");
				Assert.AreEqual(1, response.userId);
				Assert.AreEqual(1, response.id);
				Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
				Assert.AreEqual("quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto", response.body);
			}
		}


		[TestMethod]
		public void SaManagerCreatesHttpClientSaRedis()
		{
			var notneeded = new HttpClientSaRedis();
			var client = HttpClientSaManager.Get("p1");
			Assert.IsTrue(client.GetType() == typeof(HttpClientSaRedis));
		}
	}
	
	public class JsonPlaceholder
	{
		public int userId;
		public int id;
		public string title;
		public string body;
	}

}
