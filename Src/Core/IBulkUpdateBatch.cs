namespace CouchDude
{
	/// <summary>Bulk update changes summary interface.</summary>
	public interface IBulkUpdateBatch
	{
		/// <summary>Requires provided document to be updated.</summary>
		void Update(IDocument document);

		/// <summary>Requires provided document to be created.</summary>
		void Create(IDocument document);

		/// <summary>Requires provided document to be deleted.</summary>
		void Delete(IDocument document);

		/// <summary>Requires document of provided ID and revision to be deleted.</summary>
		void Delete(string documentId, string revision);
	}
}