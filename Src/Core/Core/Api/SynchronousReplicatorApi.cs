using System.Collections.Generic;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class SynchronousReplicatorApi : ISynchronousReplicatorApi
	{
		private readonly ReplicatorApi replicatorApi;
		public SynchronousReplicatorApi(ReplicatorApi replicatorApi) { this.replicatorApi = replicatorApi; }

		public DocumentInfo SaveDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			return replicatorApi.SaveDescriptor(replicationTask).WaitForResult();
		}

		public ReplicationTaskDescriptor RequestDescriptorById(string id)
		{
			return replicatorApi.RequestDescriptorById(id).WaitForResult();
		}

		public DocumentInfo Delete(ReplicationTaskDescriptor replicationTask)
		{
			return replicatorApi.DeleteDescriptor(replicationTask).WaitForResult();
		}

		public IEnumerable<string> GetAllDescriptorNames()
		{
			return replicatorApi.GetAllDescriptorNames().WaitForResult();
		}
	}
}