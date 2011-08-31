using System.Collections.Generic;

namespace CouchDude.Impl
{
	/// <summary>CouchDB view query result class.</summary>
	public class LuceneQueryResult : QueryResult<LuceneResultRow>, ILuceneQueryResult
	{
		/// <summary>Empty query result.</summary>
		public static readonly ILuceneQueryResult Empty =
			new LuceneQueryResult(rows: new LuceneResultRow[0], totalCount: 0, offset: 0, query: new LuceneQuery());

		/// <constructor />
		public LuceneQueryResult(ICollection<LuceneResultRow> rows, int totalCount, int offset, LuceneQuery query)
			: base(rows, totalCount, offset) { Query = query; }

		/// <constructor />
		public LuceneQueryResult(IEnumerable<LuceneResultRow> rows, int count, int totalCount, int offset, LuceneQuery query)
			: base(rows, count, totalCount, offset) { Query = query; }

		/// <inheritdoc/>
		public LuceneQuery Query { get; private set; }

		/// <inheritdoc/>
		public LuceneQuery NextPageQuery { get { return null; } }
	}

	/// <summary>Typed CouchDB view query result class.</summary>
	public class LuceneQueryResult<T> : QueryResult<T, LuceneResultRow>, ILuceneQueryResult<T>
	{
		/// <summary>Empty query result.</summary>
		// ReSharper disable StaticFieldInGenericType
		public static readonly ILuceneQueryResult<T> Empty =
			new LuceneQueryResult<T>(rows: new LuceneResultRow[0], totalCount: 0, offset: 0, query: new LuceneQuery(), rowConvertor: _ => default(T));
		// ReSharper restore StaticFieldInGenericType

		/// <constructor />
		public LuceneQueryResult(ICollection<LuceneResultRow> rows, int totalCount, int offset, LuceneQuery query, RowConvertor<T, LuceneResultRow> rowConvertor)
			: base(rows, totalCount, offset, rowConvertor) { Query = query; }

		/// <constructor />
		public LuceneQueryResult(IEnumerable<LuceneResultRow> rows, int count, int totalCount, int offset, LuceneQuery query, RowConvertor<T, LuceneResultRow> rowConvertor)
			: base(rows, count, totalCount, offset, rowConvertor) { Query = query; }

		/// <inheritdoc/>
		public LuceneQuery Query { get; private set; }

		/// <inheritdoc/>
		public LuceneQuery NextPageQuery { get { return null; } }
	}
}