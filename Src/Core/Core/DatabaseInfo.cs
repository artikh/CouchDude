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
				dynamic info = dbInfo;
				DocumentCount = ValueOrDefault((int?)info.doc_count);
				UpdateSequenceNumber = ValueOrDefault((int?)info.update_seq);
				PurgeOperationsPerformed = ValueOrDefault((int?)info.purge_seq);
				DoesCompactionRunning = ValueOrDefault((bool?)info.compact_running);

				FileSizeInBytes = ValueOrDefault((int?)info.disk_size);
				FileFormatVersion = ValueOrDefault((int?)info.disk_format_version);
			}
		}

		private static T ValueOrDefault<T>(T? value) where T: struct
		{
			return value ?? default(T);
		}
	}
}