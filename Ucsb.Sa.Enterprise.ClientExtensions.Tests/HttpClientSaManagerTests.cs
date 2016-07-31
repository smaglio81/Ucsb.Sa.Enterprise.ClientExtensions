using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Tests
{
	[TestClass]
	public class HttpClientSaManagerTests
	{


		[TestMethod]
		public void AddUpdateAndUse()
		{
			AddUpdateAndUseCode();
		}

		[TestMethod]
		public void AddUpdateAndUseTracing()
		{
			try
			{
				HttpClientSaManager.TraceLevel = HttpClientSaTraceLevel.All;
				AddUpdateAndUseCode();
			}
			finally
			{
				HttpClientSaManager.TraceLevel = HttpClientSaTraceLevel.None;
			}

		}

		public void AddUpdateAndUseCode()
		{
            HttpClientSaManager.Remove("placeholder");
			HttpClientSaManager.Add("placeholder", "http://jsonplaceholder.typicode.com/posts/100");

			using(var client = HttpClientSaManager.NewClient("placeholder"))
			{
				var response = client.Get<JsonPlaceholder>();
				Assert.AreEqual(10, response.userId);
				Assert.AreEqual(100, response.id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response.title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response.body);
			}


			HttpClientSaManager.Update("placeholder", "");

			using (var client = HttpClientSaManager.NewClient("placeholder"))
			{
				var response = client.Get<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/100");
				Assert.AreEqual(10, response.userId);
				Assert.AreEqual(100, response.id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response.title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response.body);
			}


			HttpClientSaManager.Update("placeholder", "http://jsonplaceholder.typicode.com");

			using (var client = HttpClientSaManager.NewClient("placeholder"))
			{
				var response = client.Get<JsonPlaceholder>("/posts/100");
				Assert.AreEqual(10, response.userId);
				Assert.AreEqual(100, response.id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response.title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response.body);
			}
			

			HttpClientSaManager.Update("placeholder", "http://jsonplaceholder.typicode.com");

			using (var client = HttpClientSaManager.NewClient("placeholder"))
			{
				//	this is crazy, this works too
				var response = client.Get<JsonPlaceholder>("http://jsonplaceholder.typicode.com/posts/100");
			}
		}

		[TestMethod]
		public void PostTraceWithCallback()
		{
			var watch = new System.Threading.ManualResetEvent(false);
			var signaled = false;

            HttpClientSaManager.Remove("placeholder");
            HttpClientSaManager.Add(
				"placeholder",
				"http://jsonplaceholder.typicode.com",
				null,
				call => { signaled = true; watch.Set(); }
			);

			using (var client = HttpClientSaManager.NewClient("placeholder"))
			{
				client.TraceLevel = HttpClientSaTraceLevel.All;
				
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

				watch.WaitOne(20000);

				if (signaled == false)
				{
					throw new Exception("The callback did not execute within 20 seconds.");
				}
			}
		}

		[TestMethod]
		public void AddRemove()
		{
            HttpClientSaManager.Remove("placeholder");
            HttpClientSaManager.Add("a", "http://jsonplaceholder.typicode.com");

			var client = HttpClientSaManager.NewClient("a");

			Assert.IsNotNull(client);

			HttpClientSaManager.Remove("a");

			try
			{
				client = HttpClientSaManager.NewClient("a");
				throw new Exception("The client \"a\" should not have been found. This code should not have been reached.");
			} catch(Exception e)
			{
				if(!e.Message.StartsWith("No configurations were defined for name"))
				{
					throw;
				}
			}

			HttpClientSaManager.Remove("b");
		}

		[TestMethod]
		public void UseConfigFile()
		{
            var watch = new System.Threading.ManualResetEvent(false);
            var signaled = false;

            using (var client = HttpClientSaManager.NewClient("p1"))
			{
                client.PostDbLogged = call => { signaled = true; watch.Set(); };

                var response = client.Get<JsonPlaceholder>("/posts/100?qs=ignore");
				Assert.AreEqual(10, response.userId);
				Assert.AreEqual(100, response.id);
				Assert.AreEqual("at nam consequatur ea labore ea harum", response.title);
				Assert.AreEqual("cupiditate quo est a modi nesciunt soluta\nipsa voluptas error itaque dicta in\nautem qui minus magnam et distinctio eum\naccusamus ratione error aut", response.body);

                watch.WaitOne(20000);

                if (signaled == false)
                {
                    throw new Exception("The callback did not execute within 20 seconds.");
                }
            }

			using(var hclient = HttpClientSaManager.NewClient("h1"))
			{
				Assert.AreEqual("v1", hclient.RequestHeaders["n1"]);
			}
		}
	}
}
