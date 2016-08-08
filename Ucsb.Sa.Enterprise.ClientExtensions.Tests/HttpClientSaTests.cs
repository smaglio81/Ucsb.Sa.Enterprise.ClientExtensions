using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Tests
{
	[TestClass]
	public class HttpClientSaTests
	{

		[TestMethod]
		public void Get()
		{
			using(var client = new HttpClientSa())
			{
				var response = client.Get("http://jsonplaceholder.typicode.com/posts/1");
				Assert.AreEqual(
					"{\n  \"userId\": 1,\n  \"id\": 1," +
					"\n  \"title\": \"sunt aut facere repellat provident occaecati excepturi optio reprehenderit\"," +
					"\n  \"body\": \"quia et suscipit\\nsuscipit recusandae consequuntur expedita et cum\\nreprehenderit molestiae ut ut quas totam\\nnostrum rerum est autem sunt rem eveniet architecto\"\n}",
					response
				);
			}
		}

		[TestMethod]
		public void GetCached()
		{
			using (var client = new HttpClientSa())
			{
				var warmup = client.Get("http://jsonplaceholder.typicode.com/posts/1");	// prime client, but don't care about result

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
				}

				Assert.IsTrue(uncached.ElapsedTicks > cached.ElapsedTicks);
			}
		}

		[TestMethod]
		public void GetCachedAsync()
		{
			using (var client = new HttpClientSa())
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
			}
		}

		[TestMethod]
		public void GetTyped()
		{
			using (var client = new HttpClientSa())
			{
				var response = client.Get<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1");
				Assert.AreEqual(1, response.userId);
				Assert.AreEqual(1, response.id);
				Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
				Assert.AreEqual("quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto", response.body);
			}
		}

		[TestMethod]
		public void Post()
		{
			using(var client = new HttpClientSa())
			{
				var data = new JsonPlaceholder() { userId = 1, id = 1,
					title = "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
					body = "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto"
				};
				var response = client.Post("http://jsonplaceholder.typicode.com/posts/", data);
				Assert.IsNotNull(response);
			}
		}

		[TestMethod]
		public void PostTyped()
		{
			using (var client = new HttpClientSa())
			{
				PostTypedCode(client);
			}
		}

		[TestMethod]
		public void PostTypedTracing()
		{
			using (var client = new HttpClientSa())
			{
				client.TraceLevel = HttpClientSaTraceLevel.All;
				PostTypedCode(client);
			}

		}

		public void PostTypedCode(HttpClientSa client)
		{
			var data = new JsonPlaceholder()
			{
				userId = 1,
				id = 1,
				title = "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
				body = "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto"
			};
			var response = client.Post<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/", data);
			Assert.AreEqual(1, response.userId);
			Assert.AreEqual(1, response.id);
			Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
			Assert.AreEqual("quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto", response.body);
		}



		[TestMethod]
		public void PostTraceWithCallback()
		{
			var watch = new System.Threading.ManualResetEvent(false);
			var signaled = false;
			using (var client = new HttpClientSa())
			{
				client.PostDbLogged = call => { signaled = true; watch.Set(); };
				client.TraceLevel = HttpClientSaTraceLevel.All;
				PostTypedCode(client);
				watch.WaitOne(20000);

				if(signaled == false)
				{
					throw new Exception("The callback did not execute within 20 seconds.");
				}
			}
		}

		[TestMethod]
		public void Put()
		{
			using (var client = new HttpClientSa())
			{
				var data = new JsonPlaceholder()
				{
					userId = 1,
					id = 1,
					title = "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
					body = "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto"
				};
				var response = client.Put("http://jsonplaceholder.typicode.com/posts/1", data);
				Assert.IsNotNull(response);
			}
		}

		[TestMethod]
		public void PutTyped()
		{
			using (var client = new HttpClientSa())
			{
				var data = new JsonPlaceholder()
				{
					userId = 1,
					id = 1,
					title = "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
					body = "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto"
				};
				var response = client.Put<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/1", data);
				Assert.AreEqual(1, response.userId);
				Assert.AreEqual(1, response.id);
				Assert.AreEqual("sunt aut facere repellat provident occaecati excepturi optio reprehenderit", response.title);
				Assert.AreEqual("quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto", response.body);
			}
		}

		[TestMethod]
		public void Delete()
		{
			using (var client = new HttpClientSa())
			{
				var response = client.Get("http://jsonplaceholder.typicode.com/posts/100");
				Assert.IsNotNull(response);
			}
		}

		[TestMethod]
		public void DeleteTyped()
		{
			using (var client = new HttpClientSa())
			{
				var response = client.Get<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/100");
				Assert.AreEqual(10, response.userId);
				Assert.AreEqual(100, response.id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response.title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response.body);
			}
		}

		[TestMethod]
		public void GetAsync()
		{
			using (var client = new HttpClientSa())
			{
				string throwaway = client.GetAsyncAsString("http://jsonplaceholder.typicode.com/posts/100").Result;

				JsonPlaceholder response = client.GetAsync<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/100").Result;
				Assert.AreEqual(10, response.userId);
				Assert.AreEqual(100, response.id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response.title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response.body);
			}
		}

		[TestMethod]
		public void GetAsyncAwait()
		{
			GetAsyncAwaitImpl();
		}

		public async void GetAsyncAwaitImpl()
		{
			using (var client = new HttpClientSa())
			{
				//string throwaway = client.GetAsync("http://jsonplaceholder.typicode.com/posts/100").Result;
				string throwaway = await client.GetAsyncAsString("http://jsonplaceholder.typicode.com/posts/100");

				//JsonPlaceholder response = client.GetAsync<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/100").Result;
				JsonPlaceholder response = await client.GetAsync<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/100");
				Assert.AreEqual(10, response.userId);
				Assert.AreEqual(100, response.id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response.title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response.body);
			}
		}

		[TestMethod]
		public void GetAsyncList()
		{
			using (var client = new HttpClientSa())
			{
				string throwaway = client.GetAsyncAsString("http://jsonplaceholder.typicode.com/posts").Result;

				List<JsonPlaceholder> response = client.GetAsync<List<JsonPlaceholder>>("http://jsonplaceholder.typicode.com/posts").Result;
				Assert.AreEqual(10, response[99].userId);
				Assert.AreEqual(100, response[99].id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response[99].title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response[99].body);
			}
		}

		
		public async Task<List<PType>> GetAsyncGraddivList()
		{
			var headers = new Dictionary<string,string> { { "Timestamp", DateTime.Now.ToString("O") } };
			using (var client = new HttpClientSa("http://apps.dev.graddiv.ucsb.edu/webservices/financial/", headers))
			{
				List<PType> response = await client.GetAsync<List<PType>>("ptypes");
				Assert.IsNotNull(response);
				return response;
			}
		}

		[TestMethod]
		public void GetAsyncGraddivListAwait()
		{
			var s = GetAsyncGraddivList();
			s.Wait();
			Assert.IsNotNull(s);
		}

		public class PType
		{
			[Required]
			[Display(Name = "ID")]
			public int Id { get; set; }

			[Required]
			[Display(Name = "Type")]
			public string Type { get; set; }

			[Required]
			[Display(Name = "Description")]
			public string Description { get; set; }
		}

		[TestMethod]
		public void ConfigureClient()
		{
			var headers = new Dictionary<string, string> { { "X-Ignore", "TestValue" } };
			using (var client = new HttpClientSa("http://jsonplaceholder.typicode.com/", headers))
			{
				var originalCount = client.DefaultRequestHeaders.Count();

				client.ConfigureHeaders();

				Assert.AreEqual(originalCount, client.DefaultRequestHeaders.Count());
				
				headers.Add("X-Ignore-2", "SomeOtherValue");
				client.RequestHeaders = headers;
				client.ConfigureHeaders();

				Assert.AreEqual(originalCount + 1, client.DefaultRequestHeaders.Count());
			}
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
