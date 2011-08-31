namespace CouchDude
{
	/// <summary>Typed CouchDB view query result.</summary>
	public interface IViewQueryResult<out T> : IViewQueryResult, IQueryResult<T, ViewResultRow> { }

	/// <summary>CouchDB view query result.</summary>
	public interface IViewQueryResult : IQueryResult<ViewResultRow>
	{
		/// <summary>Query used to produce current results set.</summary>
		ViewQuery Query { get; }

		/// <summary>Returns next page view query or <c>null</c> if instance represents last page of results.</summary>
		ViewQuery NextPageQuery { get; }
	}
}