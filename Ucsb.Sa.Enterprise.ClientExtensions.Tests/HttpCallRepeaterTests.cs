using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ucsb.Sa.Enterprise.MvcExtensions.Tests
{
	[TestClass]
	public class HttpCallRepeaterTests
	{

		[TestMethod]
		public void RepeatById()
		{
			var response = HttpCallRepeater.Repeat(280).Result;

			Assert.IsTrue(((int)response.StatusCode) >= 200);
		}

	}
}
