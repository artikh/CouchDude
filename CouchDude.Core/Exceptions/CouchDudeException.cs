using System;
using System.Runtime.Serialization;

namespace CouchDude.Core
{
	/// <summary>Base class for all couch dude exeptions for better catching.</summary>
	public abstract class CouchDudeException: Exception
	{
		/// <constructor />
		protected CouchDudeException() {}

		/// <constructor />
		protected CouchDudeException(string message) : base(message) { }

		/// <constructor />
		protected CouchDudeException(string message, Exception innerException)
			: base(message, innerException) {}

		/// <constructor />
		protected CouchDudeException(SerializationInfo info, StreamingContext context)
			: base(info, context) {}
	}
}
