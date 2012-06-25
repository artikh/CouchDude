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
		readonly CouchApi parent;
		private readonly ISynchronousReplicatorApi synchronousReplicatorApi;
		private readonly IDatabaseApi replicatorDbApi;

		public ReplicatorApi(CouchApi parent)
		{
			this.parent = parent;
			synchronousReplicatorApi = new SynchronousReplicatorApi(this);
			replicatorDbApi = parent.Db(parent.Settings.ReplicatorDatabase);
		}

		public async Task<DocumentInfo> SaveDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			if (replicationTask == null) throw new ArgumentNullException("replicationTask");
			if (replicationTask.Id.HasNoValue()) 
				throw new ArgumentException("Replication task descriptor ID should be specified", "replicationTask");
			
			var json = (JsonObject)parent.Settings.Serializer.ConvertToJson(replicationTask);

			var doc = new Document(new JsonObject(
				from kvp in json
				where !kvp.Key.StartsWith("_replication_")
				select kvp
			));

			var docInfo = await replicatorDbApi
				.SaveDocument(doc, overwriteConcurrentUpdates: true)
				.ConfigureAwait(false);
			replicationTask.Revision = docInfo.Revision;
			return docInfo;
		}

		public async Task<ReplicationTaskDescriptor> RequestDescriptorById(string id)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");

			var doc = await replicatorDbApi.RequestDocument(id).ConfigureAwait(false);
			return doc != null? parent.Settings.Serializer.ConvertFromJson<ReplicationTaskDescriptor>(doc.RawJsonObject, throwOnError: true): null;
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

		public Task<ICollection<string>> GetAllDescriptorNames()
		{
			return SelectReplicationDescriptors(r => r.DocumentId, includeDocs: false);
		}

		public Task<ICollection<ReplicationTaskDescriptor>> GetAllDescriptors()
		{
			var serializer = parent.Settings.Serializer;
			return SelectReplicationDescriptors(
				r => serializer.ConvertFromJson<ReplicationTaskDescriptor>(r.Document.RawJsonObject, throwOnError: true), 
				includeDocs: true
			);
		}

		async Task<ICollection<T>> SelectReplicationDescriptors<T>(Func<ViewResultRow, T> transform, bool includeDocs)
		{
			var result = await replicatorDbApi
				.Query(new ViewQuery { ViewName = "_all_docs", IncludeDocs = includeDocs })
				.ConfigureAwait(false);
			return result.Rows.Where(r => !r.DocumentId.StartsWith("_design/")).Select(transform).ToArray();
		}

		public ISynchronousReplicatorApi Synchronously { get { return synchronousReplicatorApi; } }
	}
}