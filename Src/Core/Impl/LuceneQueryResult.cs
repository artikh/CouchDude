using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CouchDude.Impl
{
	/// <summary>CouchDB view query result class.</summary>
	public class LuceneQueryResult : QueryResult<LuceneResultRow>, ILuceneQueryResult
	{
		/// <summary>Empty query result.</summary>
		public static readonly ILuceneQueryResult Empty =
			new LuceneQueryResult(query: new LuceneQuery(), rows: new LuceneResultRow[0], totalCount: 0, offset: 0);
		
		/// <constructor />
		public LuceneQueryResult(LuceneQuery query, ICollection<LuceneResultRow> rows, int totalCount, int offset)
			: base(rows, totalCount, offset) { Query = query; }

		/// <constructor />
		public LuceneQueryResult(LuceneQuery query, IEnumerable<LuceneResultRow> rows, int count, int totalCount, int offset)
			: base(rows, count, totalCount, offset) { Query = query; }

		/// <inheritdoc/>
		public LuceneQuery Query { get; private set; }

		/// <inheritdoc/>
		public LuceneQuery NextPageQuery { get { return GetNextPageQuery(this); } }
		
		private static readonly ConcurrentDictionary<ILuceneQueryResult, LuceneQuery> NextPageQueries =
			new ConcurrentDictionary<ILuceneQueryResult, LuceneQuery>();
		internal static LuceneQuery GetNextPageQuery(ILuceneQueryResult queryResult)
		{
			return NextPageQueries.GetOrAdd(
				queryResult,
				r => {
					if (queryResult.Count == queryResult.TotalCount || queryResult.Count == 0)
						return null;
					else
					{
						var nextPageQuery = queryResult.Query.Clone();
						nextPageQuery.Skip += queryResult.Count;
						return nextPageQuery;
					}
				});
		}
	}

	/// <summary>Typed CouchDB view query result class.</summary>
	public class LuceneQueryResult<T> : QueryResult<T, LuceneResultRow>, ILuceneQueryResult<T>
	{
		/// <summary>Empty query result.</summary>
		// ReSharper disable StaticFieldInGenericType
		public static readonly ILuceneQueryResult<T> Empty =
			new LuceneQueryResult<T>(new LuceneQuery(), new LuceneResultRow[0], totalCount: 0, offset: 0, rowConvertor: ToNullSequence);

		private static IEnumerable<T> ToNullSequence(IEnumerable<LuceneResultRow> rows) 
		{
			return rows.Select(_ => default(T));
		}

		// ReSharper restore StaticFieldInGenericType

		/// <constructor />
		public LuceneQueryResult(
			LuceneQuery query, ICollection<LuceneResultRow> rows, int totalCount, int offset, 
			Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> rowConvertor)
			: base(rows, totalCount, offset, rowConvertor) { Query = query; }

		/// <constructor />
		public LuceneQueryResult(
			LuceneQuery query, IEnumerable<LuceneResultRow> rows, int count, int totalCount, int offset,
			Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> rowConvertor)
			: base(rows, count, totalCount, offset, rowConvertor) { Query = query; }

		/// <inheritdoc/>
		public LuceneQuery Query { get; private set; }

		/// <inheritdoc/>
		public LuceneQuery NextPageQuery { get { return LuceneQueryResult.GetNextPageQuery(this); } }
	}
}