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

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DatabaseApiRequestDocumentTests
	{
		[Fact]
		public void ShouldGetDocumentFromDbByIdCorrectly()
		{
			var httpMock = new HttpClientMock(
				new {
					_id = "doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				}.ToJsonString());

			var databaseApi = GetDatabaseApi(httpMock);
			var result = databaseApi.Synchronously.RequestDocument("doc1");

			Assert.Equal("http://example.com:5984/testdb/doc1", httpMock.Request.RequestUri.ToString());
			Assert.Equal(HttpMethod.Get, httpMock.Request.Method);
			Assert.Null(httpMock.Request.Content);
			Assert.Equal(
				new
				{
					_id = "doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				}.ToDocument(),
				result
			);
		}

		[Fact]
		public void ShouldEscapeDocumentId()
		{
			var httpMock = new HttpClientMock(new
				{
					_id = "docs/doc1",
					_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
					name = "John Smith"
				}.ToJsonString());
			var databaseApi = GetDatabaseApi(httpMock);
			databaseApi.Synchronously.RequestDocument("docs/doc1");

			Assert.Equal("http://example.com:5984/testdb/docs%2Fdoc1", httpMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var httpClient = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound) {
				Content = new StringContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}", Encoding.UTF8)
			});
			Assert.Throws<DatabaseMissingException>(
				() => GetDatabaseApi(httpClient).Synchronously.RequestDocument("entity.doc1")
			);
		}

		[Fact]
		public void ShouldNotEscapeDesignDocumentIdPrefix()
		{
			var httpMock = new HttpClientMock(
				new
					{
						_id = "_design/docs/doc1",
						_rev = "1-1a517022a0c2d4814d51abfedf9bfee7",
						name = "John Smith"
					}.ToJsonString());
			var databaseApi = GetDatabaseApi(httpMock);

			databaseApi.Synchronously.RequestDocument("_design/docs/doc1");

			Assert.Equal("http://example.com:5984/testdb/_design/docs%2Fdoc1", httpMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldThrowOnIncorrectJsonGettingDocumentById()
		{
			Assert.Throws<ParseException>(() => {
				var httpMock = new HttpClientMock("Some none-json [) content");
				var databaseApi = GetDatabaseApi(httpMock);
				databaseApi.Synchronously.RequestDocument("doc1");
			});
		}
		
		[Fact]
		public void ShouldThrowOnNullParametersGettingDocumentById()
		{
			var httpMock = new HttpClientMock(string.Empty);
			var databaseApi = GetDatabaseApi(httpMock);
			Assert.Throws<ArgumentNullException>(() => databaseApi.Synchronously.RequestDocument(null));
			Assert.Throws<ArgumentNullException>(() => databaseApi.Synchronously.RequestDocument(string.Empty));
		}

		[Fact]
		public void ShouldThrowOnEmptyResponseGettingDocumentById()
		{
			Assert.Throws<ParseException>(() => {
				var httpMock = new HttpClientMock("    ");
				var databaseApi = GetDatabaseApi(httpMock);
				databaseApi.Synchronously.RequestDocument("doc1");
			});
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenGettingDocument()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);
			var databaseApi = GetDatabaseApi(httpMock);

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(() => databaseApi.Synchronously.RequestDocument("doc1"));

			Assert.True(couchCommunicationException.Message.Contains("Something wrong detected"));
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}

		[Fact]
		public void ShouldReturnNullOn404WebExceptionWhenGettingDocument()
		{
			var httpMock = new HttpClientMock(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });
			var databaseApi = GetDatabaseApi(httpMock);

			Assert.Null(databaseApi.Synchronously.RequestDocument("doc1"));
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn400StatusCode()
		{
			var httpMock =
				new HttpClientMock(new HttpResponseMessage(HttpStatusCode.BadRequest) {
					Content = new JsonContent(new { error = "bad_request", reason = "Mock reason" }.ToJsonString())
				});

			var databaseApi = GetDatabaseApi(httpMock);

			var exception = Assert.Throws<CouchCommunicationException>(
				() => databaseApi.Synchronously.RequestDocument("doc1")
			);

			Assert.Contains("bad_request: Mock reason", exception.Message);
		}

		[Fact]
		public void ShouldRetriveSpecificDocumentRevision()
		{
			var httpMock = new HttpClientMock(Entity.CreateDocWithRevision().ToString());
			var databaseApi = GetDatabaseApi(httpMock);
			databaseApi.Synchronously.RequestDocument(Entity.StandardDocId, Entity.StandardRevision);

			Assert.Equal(
				"http://example.com:5984/testdb/entity.doc1?rev=1-1a517022a0c2d4814d51abfedf9bfee7", 
				httpMock.Request.RequestUri.ToString());
		}

		private static IDatabaseApi GetDatabaseApi(HttpClientMock httpMock)
		{
			return Factory.CreateCouchApi(new Uri("http://example.com:5984/"), httpMock).Db("testdb");
		}
	}
}
