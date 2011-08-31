using System.Collections.Generic;

namespace CouchDude.Impl
{
	/// <summary>CouchDB view query result class.</summary>
	public class ViewQueryResult: QueryResult<ViewResultRow>, IViewQueryResult
	{
		/// <summary>Empty query result.</summary>
		public static readonly IViewQueryResult Empty = 
			new ViewQueryResult(rows: new ViewResultRow[0], totalCount: 0, offset: 0, query: new ViewQuery());

		/// <constructor />
		public ViewQueryResult(ICollection<ViewResultRow> rows, int totalCount, int offset, ViewQuery query) 
			: base(rows, totalCount, offset) { Query = query; }

		/// <constructor />
		public ViewQueryResult(IEnumerable<ViewResultRow> rows, int count, int totalCount, int offset, ViewQuery query) 
			: base(rows, count, totalCount, offset) { Query = query; }

		/// <inheritdoc/>
		public ViewQuery Query { get; private set; }

		/// <inheritdoc/>
		public ViewQuery NextPageQuery { get { return null; } }
	}

	/// <summary>Typed CouchDB view query result class.</summary>
	public class ViewQueryResult<T> : QueryResult<T, ViewResultRow>, IViewQueryResult<T>
	{
		/// <summary>Empty query result.</summary>
		// ReSharper disable StaticFieldInGenericType
		public static readonly IViewQueryResult<T> Empty =
			new ViewQueryResult<T>(rows: new ViewResultRow[0], totalCount: 0, offset: 0, query: new ViewQuery(), rowConvertor: _ => default(T));
		// ReSharper restore StaticFieldInGenericType

		/// <constructor />
		public ViewQueryResult(ICollection<ViewResultRow> rows, int totalCount, int offset, ViewQuery query, RowConvertor<T, ViewResultRow> rowConvertor)
			: base(rows, totalCount, offset, rowConvertor) { Query = query; }

		/// <constructor />
		public ViewQueryResult(IEnumerable<ViewResultRow> rows, int count, int totalCount, int offset, ViewQuery query, RowConvertor<T, ViewResultRow> rowConvertor)
			: base(rows, count, totalCount, offset, rowConvertor) { Query = query; }

		/// <inheritdoc/>
		public ViewQuery Query { get; private set; }

		/// <inheritdoc/>
		public ViewQuery NextPageQuery { get { return null; } }
	}
}