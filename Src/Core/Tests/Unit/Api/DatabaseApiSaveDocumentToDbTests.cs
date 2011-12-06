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
using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Api;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiSaveDocumentToDbTests
	{
		[Fact]
		public void ShouldSaveToDbCorrectly()
		{
			MockMessageHandler mockMessageHandler;
			var databaseApi = GetDatabaseApi(
				out mockMessageHandler,
				response: new {
				  ok = true,
				  id = "doc1",
				  rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
				}.ToJsonString());

			var result = databaseApi.Synchronously.SaveDocument(new { _id = "doc1", name = "John Smith" }.ToDocument());

			Assert.Equal("http://example.com:5984/testdb/doc1", mockMessageHandler.Request.RequestUri.ToString());
			Assert.Equal(HttpMethod.Put, mockMessageHandler.Request.Method);
			Assert.Equal(new { _id = "doc1", name = "John Smith" }.ToJsonString(), mockMessageHandler.RequestBody);
			Assert.Equal(new DocumentInfo("doc1", "1-1a517022a0c2d4814d51abfedf9bfee7"), result);
		}

		[Fact]
		public void ShouldThrowOnNullParametersSavingToDb()
		{
			var couchApi = GetDatabaseApi();
			Assert.Throws<ArgumentException>(() => couchApi.Synchronously.SaveDocument(new { }.ToDocument()));
			Assert.Throws<ArgumentException>(() => couchApi.Synchronously.SaveDocument(new { _id = "" }.ToDocument()));
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.SaveDocument( null));
		}


		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var handler = new MockMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound) {
				Content = new StringContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}", Encoding.UTF8)
			});

			Assert.Throws<DatabaseMissingException>(
				() => GetDatabaseApi(handler).Synchronously.SaveDocument(SimpleEntity.CreateDocument())
			);
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonSavingToDb()
		{
			var couchApi = GetDatabaseApi(response: "Some none-json [) content");
			Assert.Throws<ParseException>(() => couchApi.Synchronously.SaveDocument(new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseSavingToDb()
		{
			var couchApi = GetDatabaseApi(response: "    ");
			Assert.Throws<ParseException>(() => couchApi.Synchronously.SaveDocument(new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenSavingToDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpClientMock = new MockMessageHandler(webExeption);
			var couchApi = GetDatabaseApi(httpClientMock);

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
					() => couchApi.Synchronously.SaveDocument(new { _id = "doc1" }.ToDocument()));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnConflict()
		{
			var httpClientMock = new MockMessageHandler(
				new HttpResponseMessage {
					StatusCode = HttpStatusCode.Conflict
				});
			var couchApi = GetDatabaseApi(httpClientMock);

			Assert.Throws<StaleObjectStateException>(
				() => couchApi.Synchronously.SaveDocument(new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowInvalidDocumentExceptionOnForbidden()
		{
			var httpClientMock = new MockMessageHandler(
				new HttpResponseMessage {
					StatusCode = HttpStatusCode.Forbidden
				});
			var couchApi = GetDatabaseApi(httpClientMock);
			Assert.Throws<InvalidDocumentException>(
				() => couchApi.Synchronously.SaveDocument(new { _id = "doc1" }.ToDocument()));
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn400StatusCode()
		{
			var httpClientMock =
				new MockMessageHandler(new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = new JsonContent(new { error = "bad_request", reason = "Mock reason" }.ToJsonString())
				});

			var couchApi = GetDatabaseApi(httpClientMock);

			var exception = Assert.Throws<CouchCommunicationException>(
				() => couchApi.Synchronously.SaveDocument(new { _id = "doc1" }.ToDocument())
			);

			Assert.Contains("bad_request: Mock reason", exception.Message);
		}

		private static IDatabaseApi GetDatabaseApi(MockMessageHandler handler)
		{
			return Factory.CreateCouchApi("http://example.com:5984/", handler).Db("tesdb");
		}

		private static IDatabaseApi GetDatabaseApi(string response = "{\"_id\":\"doc1\"}")
		{
			MockMessageHandler mockMessageHandler;
			return GetDatabaseApi(out mockMessageHandler, response);
		}

		private static IDatabaseApi GetDatabaseApi(out MockMessageHandler mockMessageHandler, string response = "")
		{
			mockMessageHandler = new MockMessageHandler(response);
			return Factory.CreateCouchApi("http://example.com:5984/", mockMessageHandler).Db("testdb");
		}
	}
}
