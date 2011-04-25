using System.Collections.Generic;

namespace CouchDude.Core.Implementation
{
	/// <summary>Simple paged list implementation.</summary>
	public class PagedList<T>: IPagedList<T> where T: class 
	{
		/// <constructor />
		public PagedList(int totalRowCount, int rowCount, IEnumerable<T> rows)
		{
			TotalRowCount = totalRowCount;
			RowCount = rowCount;
			Rows = rows;
		}

		/// <inheritdoc/>
		public int TotalRowCount { get; private set; }

		/// <inheritdoc/>
		public int RowCount { get; private set; }

		/// <inheritdoc/>
		public IEnumerable<T> Rows { get; private set; }
	}
}