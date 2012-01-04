using System;
using System.Runtime.Serialization;

namespace CouchDude
{
	/// <summary>Thrown when document attachment have not found on existing document.</summary>
	[Serializable]
	public class DocumentAttachmentNotFoundException : ConfigurationException
	{
		/// <constructor />
		public DocumentAttachmentNotFoundException(SerializationInfo info, StreamingContext context): base(info, context) { }

		/// <constructor />
		public DocumentAttachmentNotFoundException(string attachmentId, string documentId, string revision)
			: base(GenerateMessage(attachmentId, documentId, revision)) { }

		private static string GenerateMessage(string attachmentId, string documentId, string revision)
		{
			return string.Format(
				"Document {0}{1} attachment {2} have not found", documentId, string.Format("(rev:{0})", revision), attachmentId
				);
		}
	}
}