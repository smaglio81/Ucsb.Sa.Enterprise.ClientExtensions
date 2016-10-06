using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Tests
{
	[TestClass]
	public class TransactionScopeBuilderTests
	{

		[TestMethod]
		public void Create()
		{
			var ts = TransactionScopeBuilder.Create();

			// Have to debug to see the inner values.
			// I don't really want to spend the time using reflection to code this.
			var s = "temp";	
		}

	}
}
