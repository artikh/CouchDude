using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Api
{
	class ReplicatorApi : IReplicatorApi
	{
		private readonly ISerializer serializer;
		private readonly ISynchronousReplicatorApi synchronousReplicatorApi;
		private readonly IDatabaseApi replicatorDbApi;

		public ReplicatorApi(ICouchApi couchApi, ISerializer serializer)
		{
			this.serializer = serializer;
			synchronousReplicatorApi = new SynchronousReplicatorApi(this);
			replicatorDbApi = couchApi.Db("_replicator");
		}

		public async Task<DocumentInfo> SaveDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			if (replicationTask == null) throw new ArgumentNullException("replicationTask");
			if (replicationTask.Id.HasNoValue()) 
				throw new ArgumentException("Replication task descriptor ID should be specified", "replicationTask");
			
			var json = (JsonObject)serializer.ConvertToJson(replicationTask);

			var doc = new Document(new JsonObject(
				from kvp in json
				where !kvp.Key.StartsWith("_replication_")
				select kvp
			));

			var docInfo = await replicatorDbApi.SaveDocument(doc, overwriteConcurrentUpdates: true);
			replicationTask.Revision = docInfo.Revision;
			return docInfo;
		}

		public async Task<ReplicationTaskDescriptor> RequestDescriptorById(string id)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");

			var doc = await replicatorDbApi.RequestDocument(id);
			return doc != null? DeserializeReplicationDescriptor(doc): null;
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

		public async Task<ICollection<string>> GetAllDescriptorNames()
		{
			var names = await replicatorDbApi.Query(GetAllReplicationDescriptorsQuery(includeDocs: false));
			return names.Rows.Select(r => r.DocumentId).ToArray();
		}

		public async Task<ICollection<ReplicationTaskDescriptor>> GetAllDescriptors()
		{
			var result = await replicatorDbApi.Query(GetAllReplicationDescriptorsQuery(includeDocs: true));
			return result.Rows.Select(r => DeserializeReplicationDescriptor(r.Document)).ToArray();
		}

		static ViewQuery GetAllReplicationDescriptorsQuery(bool includeDocs)
		{
			return new ViewQuery { ViewName = "_all_docs", StartKey = "_design", EndKey = "_design0", IncludeDocs = includeDocs };
		}

		public ISynchronousReplicatorApi Synchronously { get { return synchronousReplicatorApi; } }

		ReplicationTaskDescriptor DeserializeReplicationDescriptor(Document doc)
		{
			return serializer.ConvertFromJson<ReplicationTaskDescriptor>(doc.RawJsonObject, throwOnError: true);
		}
	}
}