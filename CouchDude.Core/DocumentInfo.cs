namespace CouchDude.Core
{
	/// <summary>Document identity and version.</summary>
	public struct DocumentInfo
	{
		/// <summary>Document id.</summary>
		public readonly string Id;

		/// <summary>Document revision.</summary>
		public readonly string Revision;

		/// <constructor />
		public DocumentInfo(string id, string revision)
		{
			Id = id;
			Revision = revision;
		}
	}
}