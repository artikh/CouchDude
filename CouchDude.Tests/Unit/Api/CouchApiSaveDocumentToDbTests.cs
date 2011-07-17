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

using System;
using System.Net;
using CouchDude.Core.Api;
using CouchDude.Core.Http;
using CouchDude.Core;
using CouchDude.Core.Impl;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class CouchApiSaveDocumentToDbTests
	{
		[Fact]
		public void ShouldSaveToDbCorrectly()
		{
			var r = TestInMockEnvironment(
				couchApi => couchApi.SaveDocumentToDb(
					"doc1", new { _id = "doc1", name = "John Smith" }.ToJObject()
				),
				response: new {
					ok = true,
					id = "doc1", 
					rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				}.ToJson()
			);

			Assert.Equal("http://example.com:5984/testdb/doc1", r.RequestedUri);
			Assert.Equal("PUT", r.RequestedMethod);
			TestUtils.AssertSameJson(
				new { _id = "doc1", name = "John Smith" }.ToJToken(), r.RequestBody);
			TestUtils.AssertSameJson(
				new {
					ok = true,
					id = "doc1",
					rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				},
				r.Result
			);
		}

		[Fact]
		public void ShouldThrowOnNullParametersSavingToDb()
		{
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb(null, new { _id = "doc1" }.ToJObject())
			));
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("", new { _id = "doc1" }.ToJObject())
			));
			Assert.Throws<ArgumentNullException>(() => TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("doc1", null)
			));
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonSavingToDb()
		{
            Assert.Throws<ParseException>(() =>
				TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("doc1", new { _id = "doc1" }.ToJObject()),
					response: "Some none-json [) content"
				)
			);
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseSavingToDb()
		{
            Assert.Throws<ParseException>(() =>
				TestInMockEnvironment(
					couchApi => couchApi.SaveDocumentToDb("doc1", new { _id = "doc1" }.ToJObject()),
					response: "    "
				)
			);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenSavingToDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);
			var couchApi = new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb");

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
				() => couchApi.SaveDocumentToDb("doc1", new { _id = "doc1" }.ToJObject()));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}



		private static TestResult TestInMockEnvironment(
			Func<ICouchApi, JObject> doTest, string response = "")
		{
			var httpMock = new HttpClientMock(response);
			var result = doTest(
				new CouchApi(httpMock, new Uri("http://example.com:5984/"), "testdb")
			);

			var recivedRequest = httpMock.Request;
			return new TestResult
			{
				RequestedUri = recivedRequest.RequestUri.ToString(),
				RequestedMethod = recivedRequest.Method.ToString(),
				RequestBody = recivedRequest.Content.GetTextReader() == null ? null : recivedRequest.Content.GetTextReader().ReadToEnd(),
				Result = result
			};
		}

		private class TestResult
		{
			public string RequestedUri;
			public string RequestedMethod;
			public string RequestBody;
			public JObject Result;
		}
	}
}
