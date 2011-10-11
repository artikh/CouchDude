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
using CouchDude.Http;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DatabaseApiDeleteDocumentTests
	{
		[Fact]
		public void ShouldSendDeleteRequestOnDeletion()
		{
			var httpMock = new HttpClientMock(new { ok = true, id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7" }.ToJsonString());
			IDatabaseApi databaseApi = Factory.CreateCouchApi(new Uri("http://example.com:5984/"), httpMock).Db("testdb");

			var resultObject = databaseApi.Synchronously.DeleteDocument(docId: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7");

			Assert.Equal(
				"http://example.com:5984/testdb/doc1?rev=1-1a517022a0c2d4814d51abfedf9bfee7", 
				httpMock.Request.RequestUri.ToString());
			Assert.Equal("DELETE", httpMock.Request.Method.ToString());
			Assert.Equal(new DocumentInfo(id: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7"), resultObject);
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			var httpMock = new HttpClientMock(new { ok = true }.ToJsonString());
			IDatabaseApi databaseApi = CreateCouchApi(httpMock).Db("testdb");

			Assert.Throws<ArgumentNullException>(
				() => databaseApi.Synchronously.DeleteDocument(docId: "doc1", revision: null));
			Assert.Throws<ArgumentNullException>(
				() => databaseApi.Synchronously.DeleteDocument(docId: null, revision: "1-1a517022a0c2d4814d51abfedf9bfee7"));
		}

		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var httpClient = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound) {
				Content = new StringContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}", Encoding.UTF8)
			});
			Assert.Throws<DatabaseMissingException>(
				() => CreateCouchApi(httpClient).Db("testdb").Synchronously.DeleteDocument("doc1", "rev-123")
				);
		}

		private static ICouchApi CreateCouchApi(IHttpClient httpClient)
		{
			return Factory.CreateCouchApi(new Uri("http://example.com:5984/"), httpClient);
		}
	}
}