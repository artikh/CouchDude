using System;
using Newtonsoft.Json;

namespace CouchDude
{
	/// <summary>Describes replication task.</summary>
	public class ReplicationTaskDescriptor
	{
		/// <summary>Identifier of the task. Should be unique.</summary>
		[JsonProperty("_id")]
		public string Id { get; set; }

		/// <summary>Task revision.</summary>
		[JsonProperty("_rev")]
		public string Revision { get; internal set; }

		/// <summary>Replication target URI. Could be absolute for remote databases or relative for local.</summary>
		public Uri Target { get; set; }

		/// <summary>Replication source URI. Could be absolute for remote databases or relative for local.</summary>
		public Uri Source { get; set; }

		/// <summary>Indicates that replication should go on forever.</summary>
		public bool Continuous { get; set; }

		/// <summary>Requests target to be created if does not exists.</summary>
		/// <remarks>Does not work in CouchDB v.1.1.0</remarks>
		[JsonProperty("create_target")]
		public bool CreateTarget { get; set; }

		/// <summary>Internal replication ID. Should be same for descriptors referring same <see cref="Target"/>/<see cref="Source"/> pair.</summary>
		[JsonProperty("_replication_id")]
		public string ReplicationId {get; internal set; }

		/// <summary>Current state of the replication process.</summary>
		[JsonProperty("_replication_state")]
		public ReplicationState ReplicationState { get; internal set; }

		/// <summary>Time replication have started.</summary>
		[JsonProperty("_replication_state_time")]
		public DateTime ReplicationStartTime { get; internal set; }
	}
}