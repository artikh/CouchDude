namespace CouchDude
{
	/// <summary>View query result object.</summary>
	public interface IViewQueryResult<out T> : IPagedList<T>
	{
		/// <summary>Query used to fetch this result.</summary>
		ViewQuery Query { get; }

		/// <summary>Returns query should be used to fetch next page of data; or <c>null</c> if current result 
		/// represents final page.</summary>
		ViewQuery GetNextPageQuery();
	}
}