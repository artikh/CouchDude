using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using CouchDude.Tests.Unit.Utils;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Impl
{
	public class CouchSessionSynchronizationContextTests : SynchronizationContextTestsBase
	{
		#region ApiStub
		private class ApiStub: ICouchApi, IDatabaseApi
		{
			public void Dispose() {  }
			public IDatabaseApi Db(string databaseName) { return this; }
			public Task<ICollection<string>> RequestAllDbNames() { throw new NotImplementedException(); }
			public IReplicatorApi Replicator { get { throw new NotImplementedException(); } }
			public Task<Document> RequestDocument(string documentId, string revision = null, AdditionalDocumentProperty additionalProperties = (AdditionalDocumentProperty) 0)
			{
				return Task.Factory.StartNew(() =>
				{
					Thread.Sleep(50); //preventing returning already done task
					return (Document) null;
				});
			}
			public Task<DocumentInfo> SaveDocument(Document document) { throw new NotImplementedException(); }
			public Task<DocumentInfo> SaveDocument(Document document, bool overwriteConcurrentUpdates) { throw new NotImplementedException(); }
			public Task<DocumentInfo> CopyDocument(string originalDocumentId, string originalDocumentRevision, string targetDocumentId, string targetDocumentRevision = null)
			{
				throw new NotImplementedException();
			}
			public Task<Attachment> RequestAttachment(string attachmentId, string documentId, string documentRevision = null) { throw new NotImplementedException(); }
			public Task<DocumentInfo> SaveAttachment(Attachment attachment, string documentId, string documentRevision = null) { throw new NotImplementedException(); }
			public Task<DocumentInfo> DeleteAttachment(string attachmentId, string documentId, string documentRevision) { throw new NotImplementedException(); }
			public Task<string> RequestLastestDocumentRevision(string documentId) { throw new NotImplementedException(); }
			public Task<DocumentInfo> DeleteDocument(string documentId, string revision) { throw new NotImplementedException(); }
			public Task<IViewQueryResult> Query(ViewQuery query)
			{
				return Task.Factory.StartNew(() => {
					Thread.Sleep(50); //preventing returning already done task
					return Mock.Of<IViewQueryResult>(r => r.Count == 0 && r.Rows == new ViewResultRow[0]);
				});
			}
			public Task<ILuceneQueryResult> QueryLucene(LuceneQuery query)
			{
				return Task.Factory.StartNew(() =>
				{
					Thread.Sleep(50); //preventing returning already done task
					return Mock.Of<ILuceneQueryResult>(r => r.Count == 0 && r.Rows == new LuceneResultRow[0]);
				});
			}
			public Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder)
			{
				return Task.Factory.StartNew<IDictionary<string, DocumentInfo>>(() => {
					Thread.Sleep(50); //preventing returning already done task
					return new Dictionary<string, DocumentInfo>(); 
				});
			}
			public Task Create(bool throwIfExists = true) { throw new NotImplementedException(); }
			public Task Delete() { throw new NotImplementedException(); }
			public Task<DatabaseInfo> RequestInfo() { throw new NotImplementedException(); }
			public Task UpdateSecurityDescriptor(DatabaseSecurityDescriptor securityDescriptor) { throw new NotImplementedException(); }
			ISynchronousDatabaseApi IDatabaseApi.Synchronously { get { throw new NotImplementedException(); } }
			ISynchronousCouchApi ICouchApi.Synchronously { get { throw new NotImplementedException(); } }
		}
		#endregion

		[Fact]
		public void ShouldStopShouldNotPostOnLoad()
		{
			CreateCouchSession().Load<Entity>("doc1").Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnQuery()
		{
			CreateCouchSession().Query<Entity>(new ViewQuery{ IncludeDocs = true }).Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnQueryLucene()
		{
			CreateCouchSession().QueryLucene<Entity>(new LuceneQuery{ IncludeDocs = true }).Wait();
			AssertNonePosted();
		}

		[Fact]
		public void ShouldNotPostOnStartSavingChanges()
		{
			var session = CreateCouchSession();
			session.Save(Entity.CreateStandardWithoutRevision());
			session.StartSavingChanges().Wait();
			AssertNonePosted();
		}

		static ISession CreateCouchSession()
		{
			return new CouchSession(Default.Settings, MockCouchApi());
		}

		static ICouchApi MockCouchApi()
		{
			return Mock.Of<ICouchApi>(c => c.Db("testdb") == new ApiStub());
		}
	}
}