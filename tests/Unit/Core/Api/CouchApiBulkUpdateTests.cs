using System;
using System.Collections.Generic;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchApiBulkUpdateTests
	{
		private static readonly string Response = new[] {
			new {id = "doc1", rev = "2-1a517022a0c2d4814d51abfedf9bfee7"},
			new {id = "doc2", rev = "2-1a517022a0c2d4814d51abfedf9bfee8"},
			new {id = "doc3", rev = "1-1a517022a0c2d4814d51abfedf9bfee9"},
			new {id = "doc4", rev = "1-1a517022a0c2d4814d51abfedf9bfee0"}
		}.ToJsonString();

		private static CouchApi CreateCouchApi() { return CreateCouchApi(new HttpClientMock()); }

		private static CouchApi CreateCouchApi(HttpClientMock httpClientMock)
		{
			return new CouchApi(httpClientMock, new Uri("http://example.com:5984/"), "testdb");
		}

		[Fact]
		public void ShouldThrowOnInvalidArgumentsToUnitOfWork()
		{
			var httpClientMock = new HttpClientMock(Response);

			ICouchApi couchApi = CreateCouchApi(httpClientMock);

			Func<Action<IBulkUpdateUnitOfWork>, IDictionary<string, DocumentInfo>> bulkUpdate = couchApi.Synchronously.BulkUpdate;

			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Create(null)));
			                                                             // Save of the document with revision 
			Assert.Throws<ArgumentException>(    () => bulkUpdate(x => x.Create(new { _id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8" }.ToDocument())));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Update(null)));
			                                                             // Update of the document without revision
			Assert.Throws<ArgumentException>(    () => bulkUpdate(x => x.Update(new { _id = "doc2" }.ToDocument())));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete(null)));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete("", "1-1a517022a0c2d4814d51abfedf9bfee0")));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete("doc3", "")));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete(null, "1-1a517022a0c2d4814d51abfedf9bfee0")));
			Assert.Throws<ArgumentNullException>(() => bulkUpdate(x => x.Delete("doc3", null)));
			                                                             // Delete of the document without revision
			Assert.Throws<ArgumentException>(() => bulkUpdate(x => x.Delete(new { _id = "doc3" }.ToDocument())));
		}

		[Fact]
		public void ShouldCreateUpdateCreateAndDeleteRecordsInBulkUpdateRequest()
		{
			var httpClientMock = new HttpClientMock(Response);

			ICouchApi couchApi = CreateCouchApi(httpClientMock);

			var result = couchApi.Synchronously.BulkUpdate(x => {
				x.Create(new { _id = "doc1", name = "John", age = 42 }.ToDocument());
				x.Update(new { _id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8", name = "John", age = 42 }.ToDocument());
				x.Delete(new { _id = "doc3", _rev = "1-1a517022a0c2d4814d51abfedf9bfee9", name = "John", age = 42 }.ToDocument());
				x.Delete("doc4", "1-1a517022a0c2d4814d51abfedf9bfee0");
			});

			var expectedDescriptor = new object[] {
				new {_id = "doc1", name = "John", age = 42},
				new {_id = "doc2", _rev = "1-1a517022a0c2d4814d51abfedf9bfee8", name = "John", age = 42},
				new {_id = "doc3", _rev = "1-1a517022a0c2d4814d51abfedf9bfee9", _deleted = true},
				new {_id = "doc4", _rev = "1-1a517022a0c2d4814d51abfedf9bfee0", _deleted = true}
			}.ToJsonString();

			var sendDescriptor = httpClientMock.Request.Content.ReadAsString();
			Assert.Equal(expectedDescriptor, sendDescriptor);
			
			Assert.Equal("2-1a517022a0c2d4814d51abfedf9bfee7", result["doc1"].Revision);
			Assert.Equal("2-1a517022a0c2d4814d51abfedf9bfee8", result["doc2"].Revision);
			Assert.Equal("1-1a517022a0c2d4814d51abfedf9bfee9", result["doc3"].Revision);
			Assert.Equal("1-1a517022a0c2d4814d51abfedf9bfee0", result["doc4"].Revision);
		}
		
		[Fact]
		public void ShouldThrowOnNullArguments() 
		{
			Assert.Throws<ArgumentNullException>(() => CreateCouchApi().BulkUpdate(null));
			Assert.Throws<ArgumentException>(() => CreateCouchApi().BulkUpdate(x => { }));
		}
	}
}