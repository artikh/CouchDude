using System;
using System.Runtime.Serialization;

namespace CouchDude
{
	/// <summary>Exception thrown on race condition during document force update.</summary>
	[Serializable]
	public class RaceConditionDetectedException : CouchCommunicationException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="DocumentIdMissingException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public RaceConditionDetectedException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public RaceConditionDetectedException(string documentId, string lastKnownRevision, int updateAttemptCount) 
			: base(GenerateMessage(documentId, lastKnownRevision, updateAttemptCount)) { }

		private static string GenerateMessage(string documentId, string lastKnownRevision, int updateAttemptCount)
		{
			return string.Format(
				"Race condition detected attempting to force update document with ID {0}, " 
					+ "last known revision {1}. Throwing error after {2} attempts",
				documentId,
				lastKnownRevision,
				updateAttemptCount
				);
		}
	}
}