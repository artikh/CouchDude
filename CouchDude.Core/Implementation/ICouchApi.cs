using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Implementation
{
	/// <summary>Represents lower-level CouchDB API: in-between HTTP and ISession.</summary>
	public interface ICouchApi
	{
		/// <summary>Requests CouchDB for document.</summary>
		JObject GetDocumentFromDbById(string docId);

		/// <summary>Saves new document in CouchDB.</summary>
		JObject SaveDocumentToDb(string docId, JObject document);

		/// <summary>Updates document in CouchDB.</summary>
		JObject UpdateDocumentInDb(string docId, JObject document);

		/// <summary>Retrives current document revision from database.</summary>
		string GetLastestDocumentRevision(string docId);

		/// <summary>Deletes document of provided <param name="docId"/> if it's revision
		/// is equal to provided <param name="revision"/>.</summary>
		JObject DeleteDocument(string docId, string revision);

		/// <summary>Queries CouchDB view.</summary>
		ViewResult Query(ViewQuery query);
	}
}