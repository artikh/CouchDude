using System;
using System.Collections.Generic;

namespace CouchDude.Impl
{
	/// <summary>Extension methods for <see cref="IQueryResult{T,TRow}"/> descendants.</summary>
	public static class QueryResultExtensions
	{
		/// <summary>Converts untyped query result ot typed one.</summary>
		public static ILuceneQueryResult<T> OfType<T>(this ILuceneQueryResult result, Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> rowConvertor)
		{
			return new LuceneQueryResult<T>(result.Query, result.Rows, result.Count, result.TotalCount, result.Offset, rowConvertor);
		}

		/// <summary>Converts untyped query result ot typed one.</summary>
		public static IViewQueryResult<T> OfType<T>(this IViewQueryResult result, Func<IEnumerable<ViewResultRow>, IEnumerable<T>> rowConvertor)
		{
			return new ViewQueryResult<T>(result.Query, result.Rows, result.Count, result.TotalCount, result.Offset, rowConvertor);
		}
	}
}