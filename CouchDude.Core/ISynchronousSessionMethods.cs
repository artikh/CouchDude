namespace CouchDude.Core
{
	/// <summary>CouchDB session synchronous methods interface.</summary>
	public interface ISynchronousSessionMethods
	{
		/// <summary>Queries CouchDB view, returning  paged list of  ether documents or view data items waiting for result.</summary>
		IPagedList<T> Query<T>(ViewQuery<T> query);

		/// <summary>Queries lucene-couchdb index waiting for the result.</summary>
		IPagedList<T> FulltextQuery<T>(LuceneQuery<T> query) where T : class;

		/// <summary>Loads entity from CouchDB placing in to first level cache.</summary>
		TEntity Load<TEntity>(string entityId) where TEntity : class;
	}
}