using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using CouchDude.Api;
using CouchDude.Tests.Unit.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class CouchApiSynchronizationContextTests: SynchronizationContextTestsBase
	{
		[Fact]
		public void ShouldNotPostOnRequestAllDbNames()
		{
			var handler = new MockMessageHandler(new[] {"db1", "db2"}.ToJsonFragment());
			CreateCouchApi(handler).RequestAllDbNames().Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnReplicatorDeleteDescriptor()
		{
			CreateCouchApi().Replicator.DeleteDescriptor(new ReplicationTaskDescriptor{ Id = "rd1", Revision = "rev1"}).Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnReplicatorGetAllDescriptorNames()
		{
			var handler = new MockMessageHandler(new { rows = new object[0] }.ToJsonObject());
			CreateCouchApi(handler).Replicator.GetAllDescriptorNames().Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnReplicatorGetAllDescriptors()
		{
			var handler = new MockMessageHandler(new { rows = new object[0] }.ToJsonObject());
			CreateCouchApi(handler).Replicator.GetAllDescriptors().Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnReplicatorRequestDescriptorById()
		{
			var handler = new MockMessageHandler(new { _id = "rd1" }.ToJsonObject());
			CreateCouchApi(handler).Replicator.RequestDescriptorById("rd1").Wait();
			AssertNonePosted();
		}
		
		[Fact]
		public void ShouldNotPostOnReplicatorSaveDescriptor()
		{
			CreateCouchApi().Replicator.SaveDescriptor(new ReplicationTaskDescriptor{ Id = "rd1" }).Wait();
			AssertNonePosted();
		}
		
		[Fact]
		public void ShouldNotPostOnBulkUpdate() 
		{
			var handler = new MockMessageHandler(new[] {
				new {id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7"}, 
				new {id = "doc2", rev = "2-1a517022a0c2d4814d51abfedf9bfee8"}
			}.ToJsonValue());

			CreateDbConfig(handler).BulkUpdate(b => {
				b.Create(new { _id = "doc1" }.ToDocument());
				b.Create(new { _id = "doc2", rev = "1-1a517022a0c2d4814d51abfedf9bfee9" }.ToDocument());
			}).Wait();

			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnCopyDocument() 
		{
			CreateDbConfig().CopyDocument("doc1", "rev1", "doc2", "rev2").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnCreateDatabase() 
		{
			CreateDbConfig().Create(throwIfExists: false).Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnDeleteDatabase() 
		{
			CreateDbConfig().Delete().Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnDeleteAttachment() 
		{
			CreateDbConfig().DeleteAttachment("attachment1", "doc1", "rev1").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnDeleteDocument() 
		{
			CreateDbConfig().DeleteDocument("doc1", "rev1").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnQuery() 
		{
			var handler = new MockMessageHandler(new { rows = new object[0] }.ToJsonObject());
			CreateDbConfig(handler).Query(new ViewQuery{ ViewName = "_all_docs" }).Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnQueryLucene() 
		{
			var handler = new MockMessageHandler(new { rows = new object[0] }.ToJsonObject());
			CreateDbConfig(handler).QueryLucene(new LuceneQuery{ DesignDocumentName = "d", IndexName = "i"}).Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnRequestAttachment() 
		{
			CreateDbConfig().RequestAttachment("attachment1", "doc1", "rev1").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnRequestDocument() 
		{
			CreateDbConfig().RequestDocument("doc1").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnRequestInfo() 
		{
			CreateDbConfig().RequestInfo().Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnRequestLastestDocumentRevision()
		{
			var handler = new MockMessageHandler(
				new HttpResponseMessage(HttpStatusCode.OK) {
					Headers = { ETag = new EntityTagHeaderValue("\"rev1\"") }
				});
			CreateDbConfig(handler).RequestLastestDocumentRevision("doc1").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnSaveAttachment() 
		{
			CreateDbConfig().SaveAttachment(new Attachment("attachment1"), "doc1").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnSaveDocument() 
		{
			CreateDbConfig().SaveDocument(new { _id = "doc1" }.ToDocument()).Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnUpdateSecurityDescriptor() 
		{
			CreateDbConfig().UpdateSecurityDescriptor(new DatabaseSecurityDescriptor{Readers = new NamesRoles{Names = new[]{"user1"}}}).Wait();
			AssertNonePosted();
		}
		
		static IDatabaseApi CreateDbConfig(MockMessageHandler handler = null) { return CreateCouchApi(handler).Db("testdb"); }

		private static ICouchApi CreateCouchApi(MockMessageHandler handler = null)
		{
			handler = handler ?? new MockMessageHandler(new {id = "doc1", rev = "1-1a517022a0c2d4814d51abfedf9bfee7"}.ToJsonValue());
			return new CouchApi(new CouchApiSettings("http://example.com:5984/"), handler);
		}
	}
}