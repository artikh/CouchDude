using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CouchDude.Configuration;
using CouchDude.Http;
using CouchDude.Utils;

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

			// manual generation of JSON needed to avoid sending _replicator_* properties to the server
			var documentString = string.Format(
				"{{\"_id\":\"{0}\",{1}\"source\":\"{2}\",\"target\":\"{3}\",\"create_target\":{4},\"continuous\":{5}}}",
				EscapeQuotes(replicationTask.Id),
				replicationTask.Revision.HasValue() 
					? string.Format("\"_rev\":\"{0}\",", EscapeQuotes(replicationTask.Revision)) 
					: string.Empty,
				EscapeQuotes(replicationTask.Source),
				EscapeQuotes(replicationTask.Target),
				replicationTask.CreateTarget.ToString().ToLower(),
				replicationTask.Continuous.ToString().ToLower()
			);
			return replicatorDbApi.SaveDocument(new Document(documentString));
		}

		private static string EscapeQuotes(Uri uri)
		{
			return uri == null ? string.Empty : EscapeQuotes(uri.ToString());
		}

		private static string EscapeQuotes(string str)
		{
			return string.Join(
				string.Empty, str.SelectMany(c => c == '\\' ? "\\\\" : (c == '"' ? "\\\"" : c.ToString())));
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