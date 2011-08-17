namespace CouchDude
{
	/// <summary>Bulk update unit of work interface.</summary>
	public interface IBulkUpdateUnitOfWork
	{
		/// <summary>Requires provided document to be updated.</summary>
		void Update(IDocument document);

		/// <summary>Requires provided document to be created.</summary>
		void Create(IDocument document);

		/// <summary>Requires provided document to be deleted.</summary>
		void Delete(IDocument document);

		/// <summary>Requires document of provided ID and revision to be deleted.</summary>
		void Delete(string id, string revision);
	}
}