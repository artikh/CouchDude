using System.Collections.Generic;
using CouchDude.Utils;

namespace CouchDude.Api
{
	class SynchronousReplicatorApi : ISynchronousReplicatorApi
	{
		private readonly ReplicatorApi replicatorApi;
		public SynchronousReplicatorApi(ReplicatorApi replicatorApi) { this.replicatorApi = replicatorApi; }

		public void SaveDescriptor(ReplicationTaskDescriptor replicationTask)
		{
			replicatorApi.SaveDescriptor(replicationTask).WaitForResult();
		}

		public ReplicationTaskDescriptor RequestDescriptorById(string id)
		{
			return replicatorApi.RequestDescriptorById(id).WaitForResult();
		}

		public void Delete(ReplicationTaskDescriptor replicationTask)
		{
			replicatorApi.DeleteDescriptor(replicationTask).WaitForResult();
		}

		public IEnumerable<string> GetAllDescriptorNames()
		{
			return replicatorApi.GetAllDescriptorNames().WaitForResult();
		}
	}
}