namespace CouchDude.Impl
{
	/// <summary>Extension methods for <see cref="IQueryResult{T,TRow}"/> descendants.</summary>
	public static class QueryResultExtensions
	{
		/// <summary>Converts untyped query result ot typed one.</summary>
		public static ILuceneQueryResult<T> OfType<T>(this ILuceneQueryResult result, RowConvertor<T, LuceneResultRow> rowConvertor)
		{
			return new LuceneQueryResult<T>(result.Rows, result.Count, result.TotalCount, result.Offset, result.Query, rowConvertor);
		}

		/// <summary>Converts untyped query result ot typed one.</summary>
		public static IViewQueryResult<T> OfType<T>(this IViewQueryResult result, RowConvertor<T, ViewResultRow> rowConvertor)
		{
			return new ViewQueryResult<T>(result.Rows, result.Count, result.TotalCount, result.Offset, result.Query, rowConvertor);
		}
	}
}