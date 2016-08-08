using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ucsb.Sa.Enterprise.ClientExtensions
{

	/// <summary>
	/// https://blogs.msdn.microsoft.com/pfxteam/2011/01/15/asynclazyt/
	/// 
	/// Adds Async functionality to the Lazy&lt;T&gt; class.
	/// </summary>
	/// <typeparam name="T">The type to return.</typeparam>
	public class AsyncLazy<T> : Lazy<Task<T>>
	{
		public AsyncLazy(Func<T> valueFactory) :
			base(() => Task.Factory.StartNew(valueFactory))
		{ }
		public AsyncLazy(Func<Task<T>> taskFactory) :
			base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
		{ }

		public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); }
	}
}
