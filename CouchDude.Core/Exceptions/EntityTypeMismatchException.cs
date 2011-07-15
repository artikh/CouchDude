using System;
using System.Runtime.Serialization;

namespace CouchDude.Core
{
	/// <summary> EntityTypeMismatch exception.</summary>
	[Serializable]
	public class EntityTypeMismatchException : CouchDudeException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="EntityTypeMismatchException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		protected EntityTypeMismatchException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public EntityTypeMismatchException(string documentType, Type entityType)
			: base(GenerateMessage(documentType, entityType)) { }

		/// <constructor />
		public EntityTypeMismatchException(Type cachedEntityType, Type entityType)
			: base(GenerateMessage(cachedEntityType, entityType)) { }

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
