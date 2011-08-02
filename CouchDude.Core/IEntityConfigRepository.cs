using System;
using CouchDude.Core.Configuration;

namespace CouchDude.Core
{
	/// <summary>Entity configuration</summary>
	public interface IEntityConfigRepository
	{
		/// <summary>Retrives entity configuration by entity type.</summary>
		IEntityConfig GetConfig(Type entityType);

		/// <summary>Retrives entity configuration by entity type returning <c>null</c> if none found.</summary>
		IEntityConfig TryGetConfig(Type entityType);

		/// <summary>Retrives entity configuration by document type.</summary>
		IEntityConfig GetConfig(string documentType);
	}
}