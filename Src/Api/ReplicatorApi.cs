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

		public Task<DocumentInfo> SaveDescriptor(ReplicationTaskDescriptor replicationTask)
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

			using (SyncContext.SwitchToDefault())
				return replicatorDbApi.SaveDocument(doc, overwriteConcurrentUpdates: true)
					.ContinueWith(rt => {
						var docInfo = rt.Result;
						replicationTask.Revision = docInfo.Revision;
						return docInfo;
					});
		}

		public Task<ReplicationTaskDescriptor> RequestDescriptorById(string id)
		{
			if (id.HasNoValue()) throw new ArgumentNullException("id");

			using (SyncContext.SwitchToDefault())
				return replicatorDbApi.RequestDocument(id).ContinueWith(
				rt => {
					var doc = rt.Result;
					return doc != null
						? parent.Settings.Serializer.ConvertFromJson<ReplicationTaskDescriptor>(
							doc.RawJsonObject, throwOnError: true)
						: null;
				});
		}

		public Task<DocumentInfo> DeleteDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			if (replicationTask == null) throw new ArgumentNullException("replicationTask");
			if (replicationTask.Id.HasNoValue())
				throw new ArgumentException("Replication task descriptor ID should be specified", "replicationTask");
			if (replicationTask.Revision.HasNoValue())
				throw new ArgumentException("Replication task descriptor revision should be specified", "replicationTask");

			using (SyncContext.SwitchToDefault())
				return replicatorDbApi.DeleteDocument(replicationTask.Id, replicationTask.Revision);
		}

		public Task<ICollection<string>> GetAllDescriptorNames()
		{
			using (SyncContext.SwitchToDefault())
				return SelectReplicationDescriptors(r => r.DocumentId, includeDocs: false);
		}

		public Task<ICollection<ReplicationTaskDescriptor>> GetAllDescriptors()
		{
			var serializer = parent.Settings.Serializer;
			using (SyncContext.SwitchToDefault())
				return SelectReplicationDescriptors(
					r => serializer.ConvertFromJson<ReplicationTaskDescriptor>(r.Document.RawJsonObject, throwOnError: true), 
					includeDocs: true
				);
		}

		Task<ICollection<T>> SelectReplicationDescriptors<T>(Func<ViewResultRow, T> transform, bool includeDocs)
		{
			using (SyncContext.SwitchToDefault())
				return replicatorDbApi
				.Query(new ViewQuery {ViewName = "_all_docs", IncludeDocs = includeDocs})
				.ContinueWith<ICollection<T>>(
					rt => {
						var result = rt.Result;
						return result.Rows.Where(r => !r.DocumentId.StartsWith("_design/")).Select(transform).ToArray();
					});
		}

		public ISynchronousReplicatorApi Synchronously { get { return synchronousReplicatorApi; } }
	}
}