using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Api
{
	class ReplicatorApi : IReplicatorApi
	{
		private readonly ISynchronousReplicatorApi synchronousReplicatorApi;
		private readonly IDatabaseApi replicatorDbApi;

		public ReplicatorApi(ICouchApi couchApi)
		{
			synchronousReplicatorApi = new SynchronousReplicatorApi(this);
			replicatorDbApi = couchApi.Db("_replicator");
		}

		public Task<DocumentInfo> SaveDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			if (replicationTask == null) throw new ArgumentNullException("replicationTask");
			if (replicationTask.Id.HasNoValue()) 
				throw new ArgumentException("Replication task descriptor ID should be specified", "replicationTask");
			
			var document = Document.Serialize(replicationTask);

			//TODO Avoid using internal APIs
			var jObject = ((Document)document).JsonObject;
			foreach (var propertyToRemove in
					jObject.Properties().Select(p => p.Name).Where(pn => pn.StartsWith("_replication_")).ToArray())
				jObject.Remove(propertyToRemove);

			return replicatorDbApi.SaveDocument(document, overwriteConcurrentUpdates: true).ContinueWith(
				saveTask => {
					replicationTask.Revision = saveTask.Result.Revision;
					return saveTask.Result;
				});
		}

		public Task<ReplicationTaskDescriptor> RequestDescriptorById(string id)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");

			return replicatorDbApi.RequestDocument(id).ContinueWith(
				t => {
					var result = t.Result;
					if (result != null)
					{
						return (ReplicationTaskDescriptor) result.Deserialize(typeof(ReplicationTaskDescriptor));
					}
					return null;
				});
		}

		public Task<DocumentInfo> DeleteDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			if (replicationTask == null) throw new ArgumentNullException("replicationTask");
			if (replicationTask.Id.HasNoValue())
				throw new ArgumentException("Replication task descriptor ID should be specified", "replicationTask");
			if (replicationTask.Revision.HasNoValue())
				throw new ArgumentException("Replication task descriptor revision should be specified", "replicationTask");

			return replicatorDbApi.DeleteDocument(replicationTask.Id, replicationTask.Revision);
		}

		public Task<IEnumerable<string>> GetAllDescriptorNames()
		{
			return replicatorDbApi.Query(new ViewQuery{ ViewName = "_all_docs"}).ContinueWith(
				t => t.Result.Rows.Select(row => row.DocumentId).Where(docId => !docId.StartsWith("_design/"))
			);
		}

		public ISynchronousReplicatorApi Synchronously { get { return synchronousReplicatorApi; } }
	}
}