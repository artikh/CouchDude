using System;
using System.Runtime.Serialization;

namespace CouchDude.Core
{
	/// <summary>Thrown in case of unregistred entity type.</summary>
	[Serializable]
	public class EntityTypeNotRegistredException : CouchDudeException
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
		public EntityTypeNotRegistredException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public EntityTypeNotRegistredException(Type entityType) : base(GenerateMessage(entityType)) { }

		private static string GenerateMessage(Type entityType)
		{
			return string.Format("Type {0} have not been registred.", entityType);
		}
	}
}