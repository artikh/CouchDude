#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

using System;
using System.Linq;
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
			Assert.True(exception.InnerExceptions.Any(e => e.Message == "one"));
			Assert.True(exception.InnerExceptions.Any(e => e.Message == "two"));
			Assert.True(exception.InnerExceptions.Any(e => e.Message == "three"));
			Assert.True(exception.InnerExceptions.Any(e => e.Message == "four"));
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
			Assert.Contains("73", exception.StackTrace);
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
			Assert.Contains("94", innerException.StackTrace);
		}
	}
}