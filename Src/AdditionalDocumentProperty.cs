using System;

namespace CouchDude
{
	/// <summary>Represents additional CouchDB document property.</summary>
	[Flags]
	public enum AdditionalDocumentProperty
	{
		/// <summary>If the document has attachments, special _attachments property holds a (meta-)data structure</summary>
		Attachments = 1,
		/// <summary>Revision history of the document</summary>
		RevisionHistory = 2,
		/// <summary>A list of revisions of the document, and their availability</summary>
		RevisionInfo = 4,
		/// <summary>Information about conflicts</summary>
		Conflicts = 8,
		/// <summary>Information about conflicts</summary>
		DeletedConflicts = 16
	}
}