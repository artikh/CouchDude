using System;
using System.Runtime.Serialization;

namespace CouchDude
{
	/// <summary>Exception thrown if non-existent database have been requested.</summary>
	[Serializable]
	public class DatabaseMissingException : CouchCommunicationException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="DatabaseMissingException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public DatabaseMissingException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public DatabaseMissingException(string databaseName) 
			: base(string.Format("Database {0} have not found on the server", databaseName)) { }
	}
}