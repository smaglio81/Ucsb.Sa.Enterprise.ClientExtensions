using System;
using System.Transactions;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{
	public class TransactionScopeBuilder
	{

		/// <summary>
		/// Create a new <see cref="TransactionScope" /> object with the defaults that we would like to use in SA. This is
		/// used to create a new top-level transaction, not to become a part of a new transaction.
		/// 
		/// Follows defaults from https://blogs.msdn.microsoft.com/dbrowne/2010/06/03/using-new-transactionscope-considered-harmful/
		/// and Workflow from http://particular.net/blog/transactionscope-and-async-await-be-one-with-the-flow
		/// </summary>
		/// <param name="transactionScopeOption">
		/// Defaults to <see cref="TransactionScopeOption.Required" />. An instance of the
		/// <see cref="TransactionScopeOption" /> enumeration that describes the transaction requirements associated
		/// with this transaction scope.
		/// </param>
		/// <param name="isolationLevel">
		/// Defaults to <see cref="IsolationLevel.ReadCommitted" />. The database <see cref="IsolationLevel" />.
		/// </param>
		/// <param name="timeout">
		/// Defaults to <see cref="TransactionManager.MaximumTimeout" />. This is the timeout for this transaction scope.
		/// </param>
		/// <param name="transactionScopeAsyncFlowOption">
		/// Defaults to <see cref="TransactionScopeAsyncFlowOption.Enabled" />. This will alow async/await keywords
		/// to be used by functions called within this transaction scope.
		/// http://particular.net/blog/transactionscope-and-async-await-be-one-with-the-flow
		/// </param>
		/// <returns></returns>
		public static TransactionScope Create(
			TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required,
			IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
			TimeSpan? timeout = null,
			TransactionScopeAsyncFlowOption transactionScopeAsyncFlowOption = TransactionScopeAsyncFlowOption.Enabled
		)
		{
			if (timeout == null)
			{
				timeout = TransactionManager.MaximumTimeout;
			}

			//	https://blogs.msdn.microsoft.com/dbrowne/2010/06/03/using-new-transactionscope-considered-harmful/
			var transactionOptions = new TransactionOptions()
			{
				IsolationLevel = isolationLevel,
				Timeout = (TimeSpan) timeout
			};

			return new TransactionScope(
				transactionScopeOption,
				transactionOptions,
				transactionScopeAsyncFlowOption
			);
		}
	}
}
