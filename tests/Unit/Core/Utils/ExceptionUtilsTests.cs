using System;
using System.Threading.Tasks;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Utils
{
	public class ExceptionUtilsTests
	{
		[Fact]
		public void ShouldPreserveStackTrace()
		{
			Exception exception = GetTaskException(
				Task.Factory
					.StartNew(
						() =>
						{
							throw new InvalidOperationException("inner inner inner exception");
						}
					).ContinueWith(
						t => { throw t.Exception.InnerException.PreserveStackTrace(); }
					)
				);

			Assert.IsType<InvalidOperationException>(exception.InnerException);
			Assert.Contains("18", exception.InnerException.StackTrace);
		}

		private static AggregateException GetTaskException(Task task)
		{
			AggregateException exception = null;
			task.ContinueWith(t => { exception = t.Exception; }).Wait();
			return exception;
		} 
	}
}