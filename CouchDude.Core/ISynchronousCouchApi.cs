namespace CouchDude.Core
{
	/// <summary>Represents synchronous version of low-level CouchDB API.</summary>
	public interface ISynchronousCouchApi
	{
		/// <summary>Requests CouchDB for document and waits for an answer.</summary>
		IDocument RequestDocumentById(string docId);
		
		/// <summary>Saves new document to CouchDB and waits for the result of the operation.</summary>
		IJsonFragment SaveDocumentSync(IDocument document);

		/// <summary>Updates document in CouchDB and waits for the result of the operation.</summary>
		IJsonFragment UpdateDocument(IDocument document);

		/// <summary>Retrives current document revision from database and waits for the result of the operation. </summary>
		/// <remarks><c>null</c> returned if there is no such document in database.</remarks>
		string RequestLastestDocumentRevision(string docId);

		/// <summary>Deletes document of provided <param name="docId"/> if it's revision
		/// is equal to provided <param name="revision"/> and waits for the result of the operation.</summary>
		IJsonFragment DeleteDocument(string docId, string revision);

		/// <summary>Queries CouchDB view and waits for the result of the operation.</summary>
		IPagedList<ViewResultRow> Query(ViewQuery query);

		/// <summary>Queries CouchDB view.</summary>
		IPagedList<LuceneResultRow> QueryLucene(LuceneQuery query);
	}
}