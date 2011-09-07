using System.Collections.Generic;
using System.Threading.Tasks;

namespace CouchDude
{
	/// <summary>Replicator database wrapping APIs.</summary>
	public interface IReplicatorApi
	{
		/// <summary>Creates new or updates existing replication task.</summary>
		Task SaveDescriptor(ReplicationTaskDescriptor replicationTask);

		/// <summary>Requests replication task by it's ID.</summary>
		Task<ReplicationTaskDescriptor> RequestDescriptorById(string id);

		/// <summary>Demands replication task from replicator database deletion.</summary>
		Task DeleteDescriptor(ReplicationTaskDescriptor replicationTask);

		/// <summary>Requests all replication task namess.</summary>
		Task<IEnumerable<string>> GetAllDescriptorNames();

		/// <summary>Synchronous version of API.</summary>
		ISynchronousReplicatorApi Synchronously { get; }
	}
}