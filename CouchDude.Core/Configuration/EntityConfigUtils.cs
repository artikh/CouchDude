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
	}
}