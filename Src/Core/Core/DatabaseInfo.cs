using System;
using System.Json;
using CouchDude.Utils;

namespace CouchDude
{
	/// <summary>Describes current CouchDB database state.</summary>
	public class DatabaseInfo
	{
		/// <summary>Database name.</summary>
		public readonly string Name;

		/// <summary>Indicates if database exists.</summary>
		public readonly bool Exists;

		/// <summary>Number of documents (including design documents) in the database.</summary>
		public readonly int DocumentCount;

		/// <summary>Current number of updates to the database</summary>
		public readonly int UpdateSequenceNumber;

		/// <summary>Number of purge operations</summary>
		public readonly int PurgeOperationsPerformed;

		/// <summary>Indicates, if a compaction is running</summary>
		public readonly bool DoesCompactionRunning;

		/// <summary>Current size in Bytes of the database (size of views indexes on disk are not included)</summary>
		public readonly int FileSizeInBytes;

		/// <summary>Current version of the internal database format on disk</summary>
		public readonly int FileFormatVersion;

		/// <summary>Converts database info document into easy to use object.</summary>
		public DatabaseInfo(bool exists, string name, JsonObject dbInfo = null)
		{
			Name = name;
			Exists = exists;
			if (exists && dbInfo != null)
			{
				DocumentCount = dbInfo.GetPrimitiveProperty<int>("doc_count");
				UpdateSequenceNumber = dbInfo.GetPrimitiveProperty<int>("update_seq");
				PurgeOperationsPerformed = dbInfo.GetPrimitiveProperty<int>("purge_seq");
				DoesCompactionRunning = dbInfo.GetPrimitiveProperty<bool>("compact_running");

				FileSizeInBytes = dbInfo.GetPrimitiveProperty<int>("disk_size");
				FileFormatVersion = dbInfo.GetPrimitiveProperty<int>("disk_format_version");
			}
		}
	}
}