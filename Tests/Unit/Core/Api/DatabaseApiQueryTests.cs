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
using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Api;
using CouchDude.Http;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DatabaseApiQueryTests
	{
		[Fact]
		public void ShouldMapTipicalResponseToGetAllQuery()
		{
			var httpClientMock = new HttpClientMock(new {
				total_rows = 2,
				offset = 1,
				rows = new [] {
					new {
						key = new object[] { "key", 0 },
						value = "some string",
						id = "doc1",
						doc = new { _id = "doc1" }
					}	
				}
			}.ToJsonString());

			var couchApi = GetDatabaseApi(httpClientMock);
			var result = couchApi.Synchronously.Query(
				new ViewQuery {
					ViewName = "_all_docs",
					Key = new object[] {"key", 0},
					Skip = 1,
					IncludeDocs = true
				});

			Assert.NotNull(result);
			Assert.Equal(2, result.TotalCount);
			Assert.Equal(1, result.Count);
			var firstRow = result.Rows.First();
			Assert.Equal(new object[] { "key", 0 }.ToJsonFragment(),	firstRow.Key);
			Assert.Equal("\"some string\"",														firstRow.Value.ToString());
			Assert.Equal("doc1",																			firstRow.DocumentId);
			Assert.Equal(new { _id = "doc1" }.ToDocument(),						firstRow.Document);
		}
		
		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var httpClient = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound) {
				Content = new StringContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}", Encoding.UTF8)
			});

			Assert.Throws<DatabaseMissingException>(
				() => GetDatabaseApi(httpClient).Synchronously.Query(new ViewQuery{
					ViewName = "_all_docs",
					Key = "key"
				})
			);
		}

		[Fact]
		public void ShouldSendGetRequestForGetAllQuery()
		{
			var httpClientMock = new HttpClientMock(new {
				total_rows = 2,
				offset = 1,
				rows = new object [0]	
				}
			.ToJsonString());

			var couchApi = GetDatabaseApi(httpClientMock);
			couchApi.Synchronously.Query(
				new ViewQuery {
					DesignDocumentName = "dd",
					ViewName = "v1",
					Skip = 1,
					IncludeDocs = true
				});


			Assert.Equal(HttpMethod.Get, httpClientMock.Request.Method);
			Assert.Equal(
				"http://example.com:5984/testdb/_design/dd/_view/v1?skip=1&include_docs=true", 
				httpClientMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldThrowOnNullQuery()
		{
			var couchApi = GetDatabaseApi();
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.Query(null));
		}

		[Fact]
		public void ShouldThrowOnSkipMoreThen9()
		{
			var couchApi = GetDatabaseApi();
			Assert.Throws<ArgumentException>(() => couchApi.Synchronously.Query(new ViewQuery { Skip = 10 }));
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn400StatusCode()
		{
			var httpClientMock =
				new HttpClientMock(new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = new JsonContent(new { error = "bad_request", reason = "Mock reason" }.ToJsonString())
				});

			var couchApi = GetDatabaseApi(httpClientMock);

			var exception = Assert.Throws<CouchCommunicationException>(
				() => couchApi.Synchronously.Query(
					new ViewQuery {
						ViewName = "_all_docs",
						Key = new object[] {"key", 0},
						Skip = 1,
						IncludeDocs = true
					})
				);

			Assert.Contains("bad_request: Mock reason", exception.Message);
		}

		private static IDatabaseApi GetDatabaseApi(IHttpClient httpClientMock = null)
		{
			return Factory.CreateCouchApi("http://example.com:5984/", httpClientMock ?? new HttpClientMock()).Db("testdb");
		}
	}
}