namespace CouchDude
{
	/// <summary>Typed CouchDB view query result.</summary>
	public interface ILuceneQueryResult<out T> : ILuceneQueryResult, IQueryResult<T, LuceneResultRow> { }

	/// <summary>CouchDB view query result.</summary>
	public interface ILuceneQueryResult : IQueryResult<LuceneResultRow>
	{
		/// <summary>Query used to produce current results set.</summary>
		LuceneQuery Query { get; }

		/// <summary>Returns next page query or <c>null</c> if instance represents last page of results.</summary>
		LuceneQuery NextPageQuery { get; }
	}
}