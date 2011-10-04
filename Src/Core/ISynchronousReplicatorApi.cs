using System.Collections.Generic;

namespace CouchDude
{
	/// <summary>Replicator database wrapping APIs.</summary>
	public interface ISynchronousReplicatorApi
	{
		/// <summary>Schedules new or updates existing replication task.</summary>
		DocumentInfo SaveDescriptor(ReplicationTaskDescriptor replicationTask);

		/// <summary>Retrives replication task by it's ID.</summary>
		ReplicationTaskDescriptor RequestDescriptorById(string id);

		/// <summary>Removes replication task from replicator database.</summary>
		DocumentInfo Delete(ReplicationTaskDescriptor replicationTask);

		/// <summary>Retrives all replication task names.</summary>
		IEnumerable<string> GetAllDescriptorNames();
	}
}