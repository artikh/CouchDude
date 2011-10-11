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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Api;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class DatabaseApiBulkUpdateTests
	{
		private static ICouchApi CreateCouchApi(HttpClientMock httpClientMock = null)
		{
			return Factory.CreateCouchApi("http://example.com:5984/", httpClientMock);
		}

		[Fact]
		public void ShouldThrowOnInvalidArgumentsToUnitOfWork()
		{
			var httpClientMock = new HttpClientMock(new[] {
				new {id = "doc1", rev = "2-1a517022a0c2d4814d51abfedf9bfee7"},
				new {id = "doc2", rev = "2-1a517022a0c2d4814d51abfedf9bfee8"},
				new {id = "doc3", rev = "1-1a517022a0c2d4814d51abfedf9bfee9"},
				new {id = "doc4", rev = "1-1a517022a0c2d4814d51abfedf9bfee0"}
			}.ToJsonString());

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");

			Func<Action<IBulkUpdateBatch>, IDictionary<string, DocumentInfo>> bulkUpdate =
				action => databaseApi.Synchronously.BulkUpdate(action);

			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Create(null)));
			// Save of the document with revision 
			Assert.Throws<ArgumentException>(    () => bulkUpdate(x => x.Create(new { _id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8" }.ToDocument())));
			Assert.Throws<ArgumentException>(    () => bulkUpdate(x => x.Create(new {  }.ToDocument())));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Update(null)));
			// Update of the document without revision
			Assert.Throws<ArgumentException>(    () => bulkUpdate(x => x.Update(new { _id = "doc2" }.ToDocument())));
			Assert.Throws<ArgumentException>(    () => bulkUpdate(x => x.Update(new { }.ToDocument())));

			Assert.Throws<ArgumentException>(() => bulkUpdate(x => x.Delete(new { }.ToDocument())));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete(null)));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete("", "1-1a517022a0c2d4814d51abfedf9bfee0")));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete("doc3", "")));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete(null, "1-1a517022a0c2d4814d51abfedf9bfee0")));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete("doc3", null)));
			// Delete of the document without revision
			Assert.Throws<ArgumentException>(() => bulkUpdate(x => x.Delete(new { _id = "doc3" }.ToDocument())));
		}

		[Fact]
		public void ShouldThrowOnNullArguments() 
		{
			Assert.Throws<ArgumentNullException>(() => CreateCouchApi().Db("testdb").BulkUpdate(null));
		}

		[Fact]
		public void ShouldNotDoNetworkingIfNoWorkToDo()
		{
			var httpClient = new HttpClientMock();
			CreateCouchApi(httpClient).Db("testdb").BulkUpdate(x => { });

			Assert.Null(httpClient.Request);
		}

		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var httpClient = new HttpClientMock(new HttpResponseMessage(HttpStatusCode.NotFound) {
				Content = new StringContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}", Encoding.UTF8)
			});
			Assert.Throws<DatabaseMissingException>(
				() => CreateCouchApi(httpClient).Db("testdb").Synchronously.BulkUpdate(
					x => x.Create(Entity.CreateDocWithoutRevision()))
			);
		}

		[Fact]
		public void ShouldCreateUpdateCreateAndDeleteRecordsInBulkUpdateRequest()
		{
			var httpClientMock = new HttpClientMock(new[] {
				new {id = "doc1", rev = "2-1a517022a0c2d4814d51abfedf9bfee7"},
				new {id = "doc2", rev = "2-1a517022a0c2d4814d51abfedf9bfee8"},
				new {id = "doc3", rev = "1-1a517022a0c2d4814d51abfedf9bfee9"},
				new {id = "doc4", rev = "1-1a517022a0c2d4814d51abfedf9bfee0"}
			}.ToJsonString());

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");

			var result = databaseApi.Synchronously.BulkUpdate(
				x => {
					x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument());
					x.Update(new { _id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8", name = "John", age = 42 }.ToDocument());
					x.Delete(new { _id = "doc3", _rev = "1-1a517022a0c2d4814d51abfedf9bfee9", name = "John", age = 42 }.ToDocument());
					x.Delete("doc4", "1-1a517022a0c2d4814d51abfedf9bfee0");
				});

			var expectedDescriptor = new {
				docs = new object[] {
					new {_id = "doc1", name = "John", age = 42},
					new {_id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8", name = "John", age = 42},
					new {_id = "doc3", _rev = "1-1a517022a0c2d4814d51abfedf9bfee9", _deleted = true},
					new {_id = "doc4", _rev = "1-1a517022a0c2d4814d51abfedf9bfee0", _deleted = true}
				}
			}.ToJsonString();

			var sendDescriptor = httpClientMock.Request.Content.ReadAsString();
			Assert.Equal(expectedDescriptor, sendDescriptor);
			
			Assert.Equal("2-1a517022a0c2d4814d51abfedf9bfee7", result["doc1"].Revision);
			Assert.Equal("2-1a517022a0c2d4814d51abfedf9bfee8", result["doc2"].Revision);
			Assert.Equal("1-1a517022a0c2d4814d51abfedf9bfee9", result["doc3"].Revision);
			Assert.Equal("1-1a517022a0c2d4814d51abfedf9bfee0", result["doc4"].Revision);
		}

		[Fact]
		public void ShouldThrowInvalidDocumentExceptionOnErrorResponse() 
		{
			var httpClientMock = new HttpClientMock(new object[] {
				new { id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7" },
				new { id = "doc2", error = "forbidden", reason = "message" }
			}.ToJsonString());

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");

			var exception = Assert.Throws<InvalidDocumentException>(() =>
				databaseApi.Synchronously.BulkUpdate(
					x => {
						x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument());
						x.Update(new { _id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8", name = "John", age = 42 }.ToDocument());
					})
			);

			Assert.Contains("doc2", exception.Message);
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnErrorResponseWhenUpdating() 
		{
			var httpClientMock = new HttpClientMock(new object[] {
				new { id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7" },
				new { id = "doc2", error = "conflict", reason = "message" }
			}.ToJsonString());

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");

			var exception = Assert.Throws<StaleObjectStateException>(() =>
				databaseApi.Synchronously.BulkUpdate(
					x => {
						x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument());
						x.Update(new { _id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8", name = "John", age = 42 }.ToDocument());
					})
			);

			Assert.Contains("doc2", exception.Message);
			Assert.Contains("update", exception.Message);
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnErrorResponseWhenDelete() 
		{
			var httpClientMock = new HttpClientMock(new object[] {
				new { id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7" },
				new { id = "doc2", error = "conflict", reason = "message" }
			}.ToJsonString());

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");

			var exception = Assert.Throws<StaleObjectStateException>(() =>
				databaseApi.Synchronously.BulkUpdate(
					x => {
						x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument());
						x.Delete(new { _id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8", _deleted = true }.ToDocument());
					})
			);

			Assert.Contains("doc2", exception.Message);
			Assert.Contains("delete", exception.Message);
		}

		[Fact]
		public void ShouldThrowStaleObjectStateExceptionOnErrorResponseWhenCreate() 
		{
			var httpClientMock = new HttpClientMock(new object[] {
				new { id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7" },
				new { id = "doc2", error = "conflict", reason = "message" }
			}.ToJsonString());

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");

			var exception = Assert.Throws<StaleObjectStateException>(() =>
				databaseApi.Synchronously.BulkUpdate(
					x => {
						x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument());
						x.Create(new { _id = "doc2" }.ToDocument());
					})
			);

			Assert.Contains("doc2", exception.Message);
			Assert.Contains("create", exception.Message);
		}

		[Fact]
		public void ShouldThrowAggregateExceptionOnMultiplyErrorResponse() 
		{
			var httpClientMock = new HttpClientMock(new object[] {
				new { id = "doc1", error = "forbidden", reason = "message" },
				new { id = "doc2", error = "conflict", reason = "message" }
			}.ToJsonString());

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");

			var exception = Assert.Throws<AggregateException>(() =>
				databaseApi.Synchronously.BulkUpdate(
					x => {
						x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument());
						x.Create(new { _id = "doc2" }.ToDocument());
					})
			);

			Assert.Equal(2, exception.InnerExceptions.Count);
			Assert.IsType<InvalidDocumentException>(exception.InnerExceptions[0]);
			Assert.IsType<StaleObjectStateException>(exception.InnerExceptions[1]);
		}

		[Fact]
		public void ShouldThrowCouchCommunicationExceptionOn400StatusCode()
		{
			var httpClientMock =
				new HttpClientMock(new HttpResponseMessage(HttpStatusCode.BadRequest) {
					Content = new JsonContent(new { error = "bad_request", reason = "Mock reason" }.ToJsonString())
				});

			IDatabaseApi databaseApi = CreateCouchApi(httpClientMock).Db("testdb");
			
			var exception = Assert.Throws<CouchCommunicationException>(() =>
				databaseApi.Synchronously.BulkUpdate(x => x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument()))
			);

			Assert.Contains("bad_request: Mock reason", exception.Message);
		}
	}
}