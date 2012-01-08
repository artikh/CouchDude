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
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiDeleteDocumentTests
	{
		[Fact]
		public void ShouldSendDeleteRequestOnDeletion()
		{
			var handler = new MockMessageHandler(
				new { ok = true, id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7" }.ToJsonObject());
			IDatabaseApi databaseApi = Factory.CreateCouchApi(new Uri("http://example.com:5984/"), handler).Db("testdb");

			var resultObject = databaseApi.Synchronously.DeleteDocument(docId: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7");

			Assert.Equal(
				"http://example.com:5984/testdb/doc1?rev=1-1a517022a0c2d4814d51abfedf9bfee7", 
				handler.Request.RequestUri.ToString());
			Assert.Equal("DELETE", handler.Request.Method.ToString());
			Assert.Equal(new DocumentInfo(id: "doc1", revision: "1-1a517022a0c2d4814d51abfedf9bfee7"), resultObject);
		}

		[Fact]
		public void ShouldThrowOnNullArguments()
		{
			var handler = new MockMessageHandler(new { ok = true }.ToJsonObject());
			IDatabaseApi databaseApi = CreateCouchApi(handler).Db("testdb");

			Assert.Throws<ArgumentNullException>(
				() => databaseApi.Synchronously.DeleteDocument(docId: "doc1", revision: null));
			Assert.Throws<ArgumentNullException>(
				() => databaseApi.Synchronously.DeleteDocument(docId: null, revision: "1-1a517022a0c2d4814d51abfedf9bfee7"));
		}

		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var handler = new MockMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound)
			{
				Content = new JsonContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}")
			});
			Assert.Throws<DatabaseMissingException>(
				() => CreateCouchApi(handler).Db("testdb").Synchronously.DeleteDocument("doc1", "rev-123")
				);
		}

		private static ICouchApi CreateCouchApi(MockMessageHandler handler)
		{
			return Factory.CreateCouchApi(new Uri("http://example.com:5984/"), handler);
		}
	}
}