using System;
using System.Collections.Generic;
using System.Reflection;
using CouchDude.Core.Utils;

namespace CouchDude.Core.Configuration
{
	/// <summary>Entity configuration item.</summary>
	public interface IEntityConfig
	{
		/// <summary>Exec type of the entity.</summary>
		Type EntityType { get; }

		/// <summary>Document type as it shows in database.</summary>
		string DocumentType { get; }

		/// <summary>Converts document ID to entity ID.</summary>
		string ConvertDocumentIdToEntityId(string documentId);

		/// <summary>Converts entity ID to document ID.</summary>
		string ConvertEntityIdToDocumentId(string entityId);

		/// <summary>Sets entity ID property.</summary>
		bool TrySetId(object entity, string entityId);

		/// <summary>Sets entity ID property.</summary>
		bool TryGetId(object entity, out string entityId);

		/// <summary>Sets entity revision property.</summary>
		void SetRevision(object entity, string entityRevision);

		/// <summary>Sets entity revision property.</summary>
		string GetRevision(object entity);

		/// <summary>Retruns entity type members that should be ignored on 
		/// serialization and deserialization.</summary>
		IEnumerable<MemberInfo> IgnoredMembers { get; }
	}
}
