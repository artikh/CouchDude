using System.Collections.Generic;
using System.Threading.Tasks;

namespace CouchDude
{
	/// <summary>Replicator database wrapping APIs.</summary>
	public interface IReplicatorApi
	{
		/// <summary>Creates new or updates existing replication task.</summary>
		Task<DocumentInfo> SaveDescriptor(ReplicationTaskDescriptor replicationTask);

		/// <summary>Requests replication task by it's ID.</summary>
		Task<ReplicationTaskDescriptor> RequestDescriptorById(string id);

		/// <summary>Demands replication task from replicator database deletion.</summary>
		Task<DocumentInfo> DeleteDescriptor(ReplicationTaskDescriptor replicationTask);

		/// <summary>Requests all replication task names.</summary>
		Task<ICollection<string>> GetAllDescriptorNames();

		/// <summary>Requests all replication task descriptors.</summary>
		Task<ICollection<ReplicationTaskDescriptor>> GetAllDescriptors();

		/// <summary>Synchronous version of API.</summary>
		ISynchronousReplicatorApi Synchronously { get; }
	}
}