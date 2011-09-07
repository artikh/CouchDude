namespace CouchDude
{
	/// <summary>State of the replication process.</summary>
	public enum ReplicationState
	{
		/// <summary>Replication inactive.</summary>
		None = 0,
		/// <summary>Replication is in process.</summary>
		Triggered,
		/// <summary>Replication have finished. Continuous replication never reaches this state.</summary>
		Completed,
		/// <summary>Replication error have occured. See logs for details.</summary>
		Error
	}
}