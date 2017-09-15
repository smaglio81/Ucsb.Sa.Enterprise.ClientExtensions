using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ucsb.Sa.Enterprise.MvcExtensions.Tests
{
	[TestClass]
	public class HttpCallSearcherTests
	{

		[TestMethod]
		public void GetErrors()
		{
			var start = new DateTime(2015, 11, 17, 15, 20, 0);
			var end = new DateTime(2015, 11, 17, 15, 40, 0);
			var results = HttpCallSearcher.GetErrors("%local%", start, end);

			Assert.IsTrue(results.Count > 0);
		}

		[TestMethod]
		public void SearchOnServerName()
		{
			var parameters = new HttpCallSearchParameters()
			{
				Start = new DateTime(2016, 02, 05, 10, 15, 0),
				End = new DateTime(2016, 02, 05, 10, 20, 0),
				ErrorsOnly = false
			};
			parameters.ServerNames.Add("5VYSLN1");

			var results = HttpCallSearcher.Search(parameters);

			Assert.IsTrue(results.Count > 0);
		}

		[TestMethod]
		public void SearchOnIp()
		{
			var parameters = new HttpCallSearchParameters()
			{
				Start = new DateTime(2016, 02, 05, 10, 15, 0),
				End = new DateTime(2016, 02, 05, 10, 20, 0),
				ErrorsOnly = false
			};
			parameters.IPs.Add("127.0.0.2");

			var results = HttpCallSearcher.Search(parameters);

			Assert.IsTrue(results.Count > 0);
		}

	}
}
