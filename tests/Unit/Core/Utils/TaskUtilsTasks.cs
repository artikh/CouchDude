using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Utils
{
	public class TaskUtilsTasks
	{
		[Fact]
		public void ShouldUnwrapAggregateExceptionsToTheFlatList()
		{
			var task = Task.Factory
				.StartNew(
					() => {
						throw new AggregateException(
							new AggregateException(
								new AggregateException(
									new Exception("one"),
									new Exception("two")
									),
								new Exception("three"),
								new AggregateException(
									new AggregateException()
									)
								)
							);
					})
				.ContinueWith(
					t => {
						throw new AggregateException(
							t.Exception,
							new AggregateException(),
							new Exception("four")
						);
					});

			var exception = Assert.Throws<AggregateException>(() => task.WaitForResult());

			Assert.Equal(4, exception.InnerExceptions.Count);
			Assert.Equal("one", exception.InnerExceptions[0].Message);
			Assert.Equal("two", exception.InnerExceptions[1].Message);
			Assert.Equal("three", exception.InnerExceptions[2].Message);
			Assert.Equal("four", exception.InnerExceptions[3].Message);
		}

		[Fact]
		public void ShouldRethrowSingleInnerExceptionOfAggregateException()
		{
			var task = Task.Factory
				.StartNew(
					() => {
						throw new InvalidOperationException("inner inner inner exception");
					}
				)
				.ContinueWith(
					t => {
						throw new AggregateException(t.Exception);
					}
				);

			var exception = Assert.Throws<InvalidOperationException>(() => task.WaitForResult());
			Assert.Equal("inner inner inner exception", exception.Message);
			Assert.Contains("ShouldRethrowSingleInnerExceptionOfAggregateException", exception.StackTrace);
			Assert.Contains("54", exception.StackTrace);
		}

		[Fact]
		public void ShouldPreserveStackTraceOfNormalInnerExceptions() //A bit obvious, but should recheck
		{
			var task = Task.Factory
				.StartNew(
					() => {
						throw new InvalidOperationException("inner inner inner exception");
					}
				)
				.ContinueWith(
					t => {
						Debug.Assert(t.Exception != null);
						var ie = t.Exception.InnerException;
						Debug.Assert(ie != null);
						throw new ArgumentException(ie.Message, ie);
					}
				);

			var exception = Assert.Throws<ArgumentException>(() => task.WaitForResult());
			var innerException = exception.InnerException;

			Assert.IsType<InvalidOperationException>(innerException);
			Assert.Equal("inner inner inner exception", innerException.Message);
			Assert.Contains("75", innerException.StackTrace);
		}
	}
}