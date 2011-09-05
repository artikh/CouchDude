using System;
using CouchDude.Api;
using CouchDude.Http;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class CouchApiBulkUpdate
	{
		[Fact]
		public void ShouldDoUpdateInBulkFlowlessly()
		{
			const string dbName = "test";
			var couchApi = Factory.CreateCouchApi("http://localhost:5984/");

			var doc1Id = Guid.NewGuid() + ".doc1";
			var doc2Id = Guid.NewGuid() + ".doc2";
			var doc3Id = Guid.NewGuid() + ".doc3";

			var doc2Result = couchApi.Synchronously.SaveDocument(dbName, new {_id = doc2Id, name = "John Smith"}.ToDocument());
			var doc3Result = couchApi.Synchronously.SaveDocument(dbName, new {_id = doc3Id, name = "John Dow"}.ToDocument());

			var result = couchApi.Synchronously.BulkUpdate(
				dbName,
				x => {
					x.Create(new {_id = doc1Id, name = "James Scully"}.ToDocument());
					x.Update(new {_id = doc2Id, _rev = doc2Result.Revision, name = "John Smith", age = 42}.ToDocument());
					x.Delete(doc3Result.Id, doc3Result.Revision);
				});

			Assert.Equal(3, result.Count);
			Assert.Equal(doc1Id, result[doc1Id].Id);
			Assert.Equal(doc2Id, result[doc2Id].Id);
			Assert.Equal(doc3Id, result[doc3Id].Id);

			dynamic loadedDoc1 = couchApi.Synchronously.RequestDocumentById(dbName, doc1Id);
			Assert.NotNull(loadedDoc1);
			Assert.Equal("James Scully", (string)loadedDoc1.name);

			dynamic loadedDoc2 = couchApi.Synchronously.RequestDocumentById(dbName, doc2Id);
			Assert.NotNull(loadedDoc2);
			Assert.Equal("John Smith", (string)loadedDoc2.name);
			Assert.Equal(42, (int)loadedDoc2.age);

			var loadedDoc3 = couchApi.Synchronously.RequestDocumentById(dbName, doc3Id);
			Assert.Null(loadedDoc3);

			couchApi.DeleteDocument(dbName, doc1Id, (string)loadedDoc1._rev);
			couchApi.DeleteDocument(dbName, doc2Id, (string)loadedDoc2._rev);
		}
	}
}
