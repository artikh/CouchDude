using System;
using System.Threading;
using System.Threading.Tasks;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class DatabaseManagementAndReplication
	{
		[Fact]
		public void ShouldCreateDatabaseAddDocumentToItReplicateAndRead()
		{
			var dbA = "db_" + Guid.NewGuid();
			var dbB = "db_" + Guid.NewGuid();
			var savedDocument = Entity.CreateDocWithoutRevision();

			var couchApi = Factory.CreateCouchApi("http://127.0.0.1:5984/");
			couchApi.Db(dbA).Synchronously.Create();
			var docInfo = couchApi.Db(dbA).Synchronously.SaveDocument(savedDocument);

			couchApi.Db(dbB).Create();

			var replicationDescriptorId = dbA + "_to_" + dbB;

			var replicationTaskDescriptor = new ReplicationTaskDescriptor {
				Id = replicationDescriptorId,
				Target = new Uri(dbB, UriKind.Relative),
				Source = new Uri(dbA, UriKind.Relative)
			};
			couchApi.Replicator.Synchronously.SaveDescriptor(replicationTaskDescriptor);
			
			var replicationDoneEvent = new ManualResetEvent(false);
			WatchForReplicationToFinish(
				() => couchApi.Replicator.RequestDescriptorById(replicationDescriptorId), replicationDoneEvent);
			replicationDoneEvent.WaitOrThrowOnTimeout();

			var loadedDocument = couchApi.Db(dbB).Synchronously.RequestDocument(savedDocument.Id);

			Assert.Equal(savedDocument.Id, loadedDocument.Id);
			Assert.Equal(docInfo.Revision, loadedDocument.Revision);

			couchApi.Replicator.DeleteDescriptor(replicationTaskDescriptor);
			couchApi.Db(dbA).Synchronously.Delete();
			couchApi.Db(dbB).Synchronously.Delete();
		}

		private static void WatchForReplicationToFinish(
			Func<Task<ReplicationTaskDescriptor>> requestDescriptor, EventWaitHandle replicationDoneEvent)
		{
			requestDescriptor().ContinueWith(
				t => {
					if (t.Result == null 
						|| t.Result.ReplicationState == ReplicationState.Triggered 
							|| t.Result.ReplicationState == ReplicationState.None)
						WatchForReplicationToFinish(requestDescriptor, replicationDoneEvent);
					else if(t.Result.ReplicationState == ReplicationState.Error)
						throw new Exception("Replication error occured");
					else if (t.Result.ReplicationState == ReplicationState.Completed)
						replicationDoneEvent.Set();
				});
		}
	}
}