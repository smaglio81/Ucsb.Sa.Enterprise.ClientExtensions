using System;
using System.Net.Http;
using System.Transactions;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	/// <summary>
	/// https://code.msdn.microsoft.com/Distributed-Transactions-c7e0a8c2
	/// </summary>
	public static class HttpRequestMessageExtension
	{
		public static void AddTransactionPropagationToken(this HttpRequestMessage request, Transaction transaction = null)
		{
			Transaction t = transaction ?? Transaction.Current;
			if (t != null)
			{
				var token = TransactionInterop.GetTransmitterPropagationToken(t);
				request.Headers.Add("TransactionToken", Convert.ToBase64String(token));
			}
		}
	}
}