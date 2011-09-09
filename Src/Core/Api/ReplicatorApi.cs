using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDude.Configuration;
using CouchDude.Utils;
using Newtonsoft.Json.Linq;

namespace CouchDude.Api
{
	class ReplicatorApi : IReplicatorApi
	{
		private readonly ISynchronousReplicatorApi synchronousReplicatorApi;
		private readonly IDatabaseApi replicatorDbApi;

		private readonly IEntityConfig descriptorEntityConfig = new EntityConfig(
			typeof (ReplicationTaskDescriptor),
			entityTypeToDocumentType: et => "replicationDescriptor",
			idMember:
				new ParticularyNamedPropertyOrPubilcFieldSpecialMember(typeof (ReplicationTaskDescriptor), "Id"),
			revisionMember:
				new ParticularyNamedPropertyOrPubilcFieldSpecialMember(
					typeof (ReplicationTaskDescriptor), "Revision"),
			documentIdToEntityId: (documentId, documentType, entityType) => documentId,
			entityIdToDocumentId: (entityId, entityType, documentType) => entityId
		);

		public ReplicatorApi(ICouchApi couchApi)
		{
			synchronousReplicatorApi = new SynchronousReplicatorApi(this);
			replicatorDbApi = couchApi.Db("_replicator");
		}

		public Task SaveDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			if (replicationTask == null) throw new ArgumentNullException("replicationTask");
			if (replicationTask.Id.HasNoValue()) 
				throw new ArgumentException("Replication task descriptor ID should be specified", "replicationTask");
			
			var descriptorDocument = Document.Serialize(replicationTask, descriptorEntityConfig);

			// avoiding sending _replicator_* properties to the server, should be a better way...
			var jObject = descriptorDocument.jsonObject;
			foreach (var propertyName in
					jObject.Properties().Select(p => p.Name).Where(pn => pn.StartsWith("_replication_") || pn == "type").ToArray())
				jObject.Remove(propertyName);

			return replicatorDbApi
				.SaveDocument(descriptorDocument)
				.ContinueWith( t => { replicationTask.Revision = t.Result.Revision; });
		}
		
		public Task<ReplicationTaskDescriptor> RequestDescriptorById(string id)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");

			return replicatorDbApi.RequestDocumentById(id).ContinueWith(
				t => {
					var result = t.Result;
					if (result != null)
					{
						result["type"] = JsonFragment.JsonString("replicationDescriptor");
						return (ReplicationTaskDescriptor) result.Deserialize(descriptorEntityConfig);
					}
					return null;
				});
		}

		public Task DeleteDescriptor(ReplicationTaskDescriptor replicationTask)
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