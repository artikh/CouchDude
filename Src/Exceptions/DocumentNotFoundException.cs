using System;
using System.Runtime.Serialization;

namespace CouchDude
{
	/// <summary>Thrown when document have not found.</summary>
	[Serializable]
	public class DocumentNotFoundException : ConfigurationException
	{
		/// <constructor />
		protected DocumentNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public DocumentNotFoundException(string documentId, string revision) : base(GenerateMessage(documentId, revision)) { }

		private static string GenerateMessage(string documentId, string revision)
		{
			return string.Format("Document {0}{1} have not found", documentId, string.Format("(rev:{0})", revision));
		}
	}
}