using System.Collections.Generic;

namespace CouchDude
{
	/// <summary>Typed query result.</summary>
	public interface IQueryResult<out T, out TRow> : IQueryResult<TRow>, IEnumerable<T> where TRow : IQueryResultRow { }

	/// <summary>Query result.</summary>
	public interface IQueryResult<out TRow> where TRow : IQueryResultRow
	{
		/// <summary>Numbre of items in result.</summary>
		int Count { get; }

		/// <summary>Total count of items in query target.</summary>
		int TotalCount { get; }

		/// <summary>First result item offset in query target.</summary>
		int Offset { get; }

		/// <summary>Raw result data.</summary>
		IEnumerable<TRow> Rows { get; }
	}
}