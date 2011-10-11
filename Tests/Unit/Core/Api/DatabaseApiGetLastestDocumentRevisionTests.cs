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
using System.Net.Http.Headers;
using System.Text;
using CouchDude.Api;
using CouchDude.Http;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DatabaseApiGetLastestDocumentRevisionTests
	{
		private static HttpResponseMessage ConstructOkResponse(string etag = null)
		{
			var response = new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty)};
			if(etag != null)
				response.Headers.ETag = new EntityTagHeaderValue(string.Format("\"{0}\"", etag));
			return response;
		}

		[Fact]
		public void ShouldGetLastestDocumentRevisionCorrectly()
		{
			var response = ConstructOkResponse("1-1a517022a0c2d4814d51abfedf9bfee7");

			var httpMock = new HttpClientMock(response);
			IDatabaseApi databaseApi = GetDatabaseApi(httpMock);
			var revision = databaseApi.Synchronously.RequestLastestDocumentRevision("doc1");

			Assert.Equal("http://example.com:5984/testdb/doc1", httpMock.Request.RequestUri.ToString());
			Assert.Equal(HttpMethod.Head, httpMock.Request.Method);
			Assert.Equal(null, httpMock.Request.Content);
			Assert.Equal("1-1a517022a0c2d4814d51abfedf9bfee7", revision);
		}

		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var httpClient = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound)
			{
				Content = new StringContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}", Encoding.UTF8)
			});
			Assert.Throws<DatabaseMissingException>(
				() => GetDatabaseApi(httpClient).Synchronously.RequestLastestDocumentRevision("doc1")
			);
		}

		[Fact]
		public void ShouldThrowOnNullParametersGettingLastestDocumentRevision()
		{
			var response = ConstructOkResponse();
			var httpMock = new HttpClientMock(response);
			IDatabaseApi databaseApi = GetDatabaseApi(httpMock);

			Assert.Throws<ArgumentNullException>(() => databaseApi.Synchronously.RequestLastestDocumentRevision(""));
			Assert.Throws<ArgumentNullException>(() => databaseApi.Synchronously.RequestLastestDocumentRevision(null));
		}

		[Fact]
		public void ShouldThrowOnAbcentEtagGettingLastestDocumentRevision()
		{
			var response = ConstructOkResponse();
			var httpMock = new HttpClientMock(response);
			IDatabaseApi databaseApi = GetDatabaseApi(httpMock);

			Assert.Throws<ParseException>(() => databaseApi.Synchronously.RequestLastestDocumentRevision("doc1"));
		}

		[Fact]
		public void ShouldReturnNullIfNoDocumentFound()
		{
			var httpMock = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound));
			IDatabaseApi databaseApi = GetDatabaseApi(httpMock);

			var version = databaseApi.Synchronously.RequestLastestDocumentRevision("doc1");
			Assert.Null(version);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOnWebExceptionWhenUpdatingDocumentInDb()
		{
			var webExeption = new WebException("Something wrong detected");
			var httpMock = new HttpClientMock(webExeption);

			IDatabaseApi databaseApi = GetDatabaseApi(httpMock);

			var couchCommunicationException =
				Assert.Throws<CouchCommunicationException>(
					() => databaseApi.Synchronously.RequestLastestDocumentRevision("doc1"));

			Assert.Equal("Something wrong detected", couchCommunicationException.Message);
			Assert.Equal(webExeption, couchCommunicationException.InnerException);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn400StatusCode()
		{
			var httpClientMock =
				new HttpClientMock(new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = new JsonContent(new { error = "bad_request", reason = "Mock reason" }.ToJsonString())
				});

			IDatabaseApi databaseApi = GetDatabaseApi(httpClientMock);

			var exception = Assert.Throws<CouchCommunicationException>(
				() => databaseApi.Synchronously.RequestLastestDocumentRevision("doc1")
			);

			Assert.Contains("bad_request: Mock reason", exception.Message);
		}

		private static IDatabaseApi GetDatabaseApi(IHttpClient httpClient)
		{
			return Factory.CreateCouchApi("http://example.com:5984/", httpClient).Db("testdb");
		}
	}
}
