using System;
using System.Runtime.Serialization;

namespace CouchDude.Core
{
	/// <summary>Thrown in case of unregistred document type.</summary>
	[Serializable]
	public class DocumentTypeNotRegistredException : ConfigurationException
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
		public DocumentTypeNotRegistredException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public DocumentTypeNotRegistredException(string documentType) : base(GenerateMessage(documentType)) { }

		private static string GenerateMessage(string documentType)
		{
			return string.Format("Unknown document type '{0}'.", documentType);
		}
	}
}