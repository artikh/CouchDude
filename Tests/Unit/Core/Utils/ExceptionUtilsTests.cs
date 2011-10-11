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
			Assert.Contains("36", exception.InnerException.StackTrace);
		}

		private static AggregateException GetTaskException(Task task)
		{
			AggregateException exception = null;
			task.ContinueWith(t => { exception = t.Exception; }).WaitOrThrowOnTimeout();
			return exception;
		} 
	}
}