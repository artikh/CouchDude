using System;

namespace CouchDude.Core
{
	public partial class EntityTypeMismatchException
	{
		/// <constructor />
		public EntityTypeMismatchException(string documentType, Type entityType)
			: this(GenerateMessage(documentType, entityType)) { }

		/// <constructor />
		public EntityTypeMismatchException(Type cachedEntityType, Type entityType)
			: this(GenerateMessage(cachedEntityType, entityType)) { }

		private static string GenerateMessage(string documentType, Type entityType)
		{
			return string.Format(
				"Document type '{0}' is incompatible with entity type {1}", 
				documentType, 
				entityType.AssemblyQualifiedName);
		}

		private static string GenerateMessage(Type cachedEntityType, Type entityType)
		{
			return string.Format(
				"Cached entity type '{0}' is incompatible with entity type {1}",
				cachedEntityType.AssemblyQualifiedName, 
				entityType.AssemblyQualifiedName);
		}
	}
}
