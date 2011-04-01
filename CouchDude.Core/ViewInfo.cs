namespace CouchDude.Core
{
	/// <summary>CouchDB view descriptor.</summary>
	public class ViewInfo
	{
		/// <summary>Name of the database.</summary>
		public readonly string DesignDocumentId;

		/// <summary>Name of the database.</summary>
		public readonly string ViewName;

		/// <constructor />
		public ViewInfo(string designDocumentId, string viewName)
		{
			DesignDocumentId = designDocumentId;
			ViewName = viewName;
		}
	}
}