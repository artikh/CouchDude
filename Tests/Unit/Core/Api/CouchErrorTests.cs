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

using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchErrorTests
	{
		private static string ProccessResponse(string responseString)
		{
			var error =
				new CouchError(
					new HttpResponseMessage(HttpStatusCode.InternalServerError) {
						Content = new StringContent(responseString, Encoding.UTF8)
					});
			return error.ToString();
		}

		[Fact]
		public void ShouldReturnResultAsIsIfNotAJson()
		{
			Assert.Equal("InternalServerError: Some none-JSON string {{ [500]", ProccessResponse("Some none-JSON string {{"));
		}

		[Fact]
		public void ShouldReturnOnlyReasonIfNoError()
		{
			Assert.Equal("InternalServerError: some reason message [500]", ProccessResponse(@"{ ""reason"": ""some reason message"" }"));
		}

		[Fact]
		public void ShouldReturnOnlyErrorAndReasonIfBothPresent()
		{
			Assert.Equal(
				"some error name: some reason message [500]",
				ProccessResponse(@"{ ""error"": ""some error name"", ""reason"": ""some reason message"" }"));
		}
	}
}
