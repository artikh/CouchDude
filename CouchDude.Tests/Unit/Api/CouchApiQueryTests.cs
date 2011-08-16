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
using System.Linq;
using System.Net.Http;
using CouchDude.Core;
using CouchDude.Core.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class CouchApiQueryTests
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

			ICouchApi couchApi = new CouchApi(httpClientMock, new Uri("http://example.com:5984/"), "testdb");
			var result = couchApi.Synchronously.Query(new ViewQuery {
				ViewName = "_all_docs",
				Key = new object[] { "key", 0 },
				Skip = 1,
				IncludeDocs = true
			});

			Assert.NotNull(result);
			Assert.Equal(2, result.TotalRowCount);
			Assert.Equal(1, result.RowCount);
			var firstRow = result.First();
			Assert.Equal(new object[] { "key", 0 }.ToJsonFragment(),	firstRow.Key);
			Assert.Equal("\"some string\"",														firstRow.Value.ToString());
			Assert.Equal("doc1",																			firstRow.DocumentId);
			Assert.Equal(new { _id = "doc1" }.ToDocument(),						firstRow.Document);
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

			ICouchApi couchApi = new CouchApi(httpClientMock, new Uri("http://example.com:5984/"), "testdb");
			couchApi.Synchronously.Query(new ViewQuery {
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
			ICouchApi couchApi = new CouchApi(new HttpClientMock(), new Uri("http://example.com:5984/"), "testdb");
			Assert.Throws<ArgumentNullException>(() => couchApi.Synchronously.Query(null));
		}

		[Fact]
		public void ShouldThrowOnSkipMoreThen9()
		{
			ICouchApi couchApi = new CouchApi(new HttpClientMock(), new Uri("http://example.com:5984/"), "testdb");
			Assert.Throws<ArgumentException>(() => couchApi.Synchronously.Query(new ViewQuery { Skip = 10 }));
		}
	}
}