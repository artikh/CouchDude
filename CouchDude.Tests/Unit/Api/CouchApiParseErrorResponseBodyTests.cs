#region Licence Info 
/*
  Copyright 2011 · Artem Tikhomirov																					
 																																					
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

using System.IO;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class CouchApiParseErrorResponseBodyTests
	{
		[Fact]
		public void ShouldReturnNullWhenParsingIncorretResultBody()
		{
			using (var textReader = new StringReader("Some none-JSON string"))
				Assert.Null(CouchApi.ParseErrorResponseBody(textReader));
		}

		[Fact]
		public void ShouldReturnOnlyErrorIfNoReason()
		{
			using (var textReader = new StringReader(@"{ ""error"": ""some error name"" }"))
				Assert.Equal("some error name", CouchApi.ParseErrorResponseBody(textReader));
		}

		[Fact]
		public void ShouldReturnOnlyReasonIfNoError()
		{
			using (var textReader = new StringReader(@"{ ""reason"": ""some reason message"" }"))
				Assert.Equal("some reason message", CouchApi.ParseErrorResponseBody(textReader));
		}

		[Fact]
		public void ShouldReturnOnlyErrorAndReasonIfBothPresent()
		{
			using (var textReader = new StringReader(
				@"{ ""error"": ""some error name"", ""reason"": ""some reason message"" }"))
				Assert.Equal(
					"some error name: some reason message",
					CouchApi.ParseErrorResponseBody(textReader));
		}

	}
}
