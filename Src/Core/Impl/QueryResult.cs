using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CouchDude.Impl
{
	/// <summary>Base class for typed query results.</summary>
	public abstract class QueryResult<T, TRow> : QueryResult<TRow>, IQueryResult<T, TRow> where TRow : IQueryResultRow
	{
		private readonly RowConvertor<T, TRow> rowConvertor;
		private readonly object syncHandle = new object();
		private volatile IEnumerable<T> convertedRows;

		/// <constructor />
		protected QueryResult(ICollection<TRow> rows, int totalCount, int offset, RowConvertor<T, TRow> rowConvertor)
			: base(rows, totalCount, offset)
		{
			this.rowConvertor = rowConvertor;
		}

		/// <constructor />
		protected QueryResult(IEnumerable<TRow> rows, int count, int totalCount, int offset, RowConvertor<T, TRow> rowConvertor)
			: base(rows, count, totalCount, offset)
		{
			this.rowConvertor = rowConvertor;
		}

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			if (convertedRows == null)
				lock (syncHandle)
					if (convertedRows == null)
						convertedRows = Rows.Select(row => rowConvertor(row));
			return convertedRows.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	/// <summary>Base class for untyped query results.</summary>
	public abstract class QueryResult<TRow> : IQueryResult<TRow> where TRow : IQueryResultRow
	{
		private readonly IEnumerable<TRow> rows = Enumerable.Empty<TRow>();

		private readonly int count;
		private readonly int totalCount;
		private readonly int offset;

		/// <constructor />
		protected QueryResult(ICollection<TRow> rows, int totalCount, int offset)
			: this(rows, rows.Count, totalCount, offset) { }

		/// <constructor />
		protected QueryResult(IEnumerable<TRow> rows, int count, int totalCount, int offset)
		{
			if (offset < 0) throw new ArgumentOutOfRangeException("offset", offset, "Offset should be positive number.");
			if (count < 0) throw new ArgumentOutOfRangeException("count", offset, "Count should be positive number.");
			if (totalCount < 0) throw new ArgumentOutOfRangeException("totalCount", offset, "Total count should be positive number.");
			if (rows == null) throw new ArgumentNullException("rows");

			this.rows = new ReadOnlyCollection<TRow>(rows.ToList());
			this.offset = offset;
			this.count = count;
			this.totalCount = totalCount;
		}

		/// <inheritdoc/>
		public int Count { get { return count; } }
		/// <inheritdoc/>
		public int TotalCount { get { return totalCount; } }
		/// <inheritdoc/>
		public int Offset { get { return offset; } }
		/// <inheritdoc/>
		public IEnumerable<TRow> Rows { get { return rows; } }
	}
}