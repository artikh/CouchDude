using System;

namespace CouchDude.Core.Configuration
{
	/// <summary>Entension methods over <see cref="IEntityConfig"/> interface.</summary>
	public static class EntityConfigUtils
	{
		/// <summary>Checks if entity is compatible with provided type.</summary>
		public static bool IsCompatibleWith<TEntity>(this IEntityConfig self) where TEntity : class
		{
			return typeof(TEntity).IsAssignableFrom(self.EntityType);
		}

		/// <summary>Retrives entity identifier throwing execption it could not be retrived.</summary>
		public static string GetId(this IEntityConfig entityConfig, object entity)
		{
			string id;
			if (!entityConfig.TryGetId(entity, out id))
				throw new ConfigurationException("Entity ID colud not be retrived using current configuration.");
			return id;
		}
	}
}