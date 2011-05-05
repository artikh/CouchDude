using System.Collections.Generic;

namespace CouchDude.Core
{
	/// <summary>Paginated list of view query results.</summary>
	public interface IPagedList<out T>: IEnumerable<T>
	{
		/// <summary>Total row in view count.</summary>
		int TotalRowCount { get; }

		/// <summary>Row in current result count.</summary>
		int RowCount { get; }
	}
}