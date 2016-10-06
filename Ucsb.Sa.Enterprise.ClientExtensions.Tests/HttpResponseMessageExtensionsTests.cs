using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Tests
{
	[TestClass]
	public class HttpResponseMessageExtensionsTests
	{

		public class DesTest
		{
			public int id { get; set; }
			public string name { get; set; }
		}

		[TestMethod]
		public void DeserializeHttpResponse_Class()
		{
			var input = "{\"id\": 1, \"name\": \"blah\"}";
			var result = HttpResponseMessageExtensions.DeserializeHttpResponse<DesTest>(input);

			Assert.AreEqual(1, result.id);
			Assert.AreEqual("blah", result.name);
		}

		[TestMethod]
		public void DeserializeHttpResponse_ValueType()
		{
			var input = "1";
			var result = HttpResponseMessageExtensions.DeserializeHttpResponse<int>(input);

			Assert.AreEqual(1, result);

			var input2 = "\"A long string of nothing\"";
			var result2 = HttpResponseMessageExtensions.DeserializeHttpResponse<string>(input2);

			Assert.AreEqual("A long string of nothing", result2);
		}

	}
}
