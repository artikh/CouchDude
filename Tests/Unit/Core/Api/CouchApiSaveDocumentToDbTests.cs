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
using System.Net.Http;
using CouchDude.Api;
using CouchDude.Http;

using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchApiSaveDocumentToDbTests
	{
		[Fact]
		public void ShouldSaveToDbCorrectly()
		{
			HttpClientMock httpClientMock;
			var couchApi = CreateCouchApi(
				out httpClientMock,
				response: new {
				  ok = true,
				  id = "doc1",
				  rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				}.ToJsonString());

			var result = couchApi.Synchronously.SaveDocument("testdb", new { _id = "doc1", name = "John Smith" }.ToDocument());

			Assert.Equal("http://example.com:5984/testdb/doc1", httpClientMock.Request.RequestUri.ToString());
			Assert.Equal(HttpMethod.Put, httpClientMock.Request.Method);
			var requestBodyReader = httpClientMock.Request.Content.GetTextReader();
			Assert.NotNull(requestBodyReader);
			Assert.Equal(new { _id = "doc1", name = "John Smith" }.ToJsonString(), requestBodyReader.ReadToEnd());
			Assert.Equal(new DocumentInfo("doc1", "1-1a517022a0c2d4814d51abfedf9bfee7"), result);
		}

		[Fact]
		public void ShouldThrowOnNullParametersSavingToDb()
		{
			var couchApi = CreateCouchApi();
			Assert.Throws<ArgumentException>(() => couchApi.Synchronously.SaveDocument("testdb", new { }.ToDocument()));
			Assert.Throws<ArgumentException>(() => couchApi.Synchronously.SaveDocument("testdb", new { _id = "" }.ToDocument()));
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.SaveDocument("testdb", null));
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.SaveDocument("", new { _id = "doc1", name = "John Smith" }.ToDocument()));
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.SaveDocument(null, new { _id = "doc1", name = "John Smith" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonSavingToDb()
		{
			var couchApi = CreateCouchApi(response: "Some none-json [) content");
			Assert.Throws<ParseException>(() => couchApi.Synchronously.SaveDocument("testdb", new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseSavingToDb()
		{
			var couchApi = CreateCouchApi(response: "    ");
			Assert.Throws<ParseException>(() => couchApi.Synchronously.SaveDocument("testdb", new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenSavingToDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpClientMock = new HttpClientMock(webExeption);
			var couchApi = CreateCouchApi(httpClientMock);

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
					() => couchApi.Synchronously.SaveDocument("testdb", new { _id = "doc1" }.ToDocument()));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict()
		{
			var httpClientMock = new HttpClientMock(
				new HttpResponseMessage {
					StatusCode = HttpStatusCode.Conflict
				});
			var couchApi = CreateCouchApi(httpClientMock);

			Assert.Throws<StaleObjectStateException>(
				() => couchApi.Synchronously.SaveDocument("testdb", new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowInvalidDocumentExceptionOnForbidden()
		{
			var httpClientMock = new HttpClientMock(
				new HttpResponseMessage {
					StatusCode = HttpStatusCode.Forbidden
				});
			var couchApi = CreateCouchApi(httpClientMock);
			Assert.Throws<InvalidDocumentException>(
				() => couchApi.Synchronously.SaveDocument("testdb", new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn400StatusCode()
		{
			var httpClientMock =
				new HttpClientMock(new HttpResponseMessage(HttpStatusCode.BadRequest, "")
				{
					Content = new JsonContent(new { error = "bad_request", reason = "Mock reason" }.ToJsonString())
				});

			var couchApi = CreateCouchApi(httpClientMock);

			var exception = Assert.Throws<CouchCommunicationException>(
				() => couchApi.Synchronously.SaveDocument("testdb", new { _id = "doc1" }.ToDocument())
			);

			Assert.Contains("bad_request: Mock reason", exception.Message);
		}

		private static ICouchApi CreateCouchApi(IHttpClient httpClientMock)
		{
			return new CouchApi(httpClientMock, new Uri("http://example.com:5984/"));
		}

		private static ICouchApi CreateCouchApi(string response = "{\"_id\":\"doc1\"}")
		{
			HttpClientMock httpClientMock;
			return CreateCouchApi(out httpClientMock, response);
		}

		private static ICouchApi CreateCouchApi(out HttpClientMock httpClientMock, string response = "")
		{
			httpClientMock = new HttpClientMock(response);
			return new CouchApi(httpClientMock, new Uri("http://example.com:5984/"));
		}
	}
}
