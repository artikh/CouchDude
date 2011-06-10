using System.Collections;
using System.Collections.Generic;

namespace CouchDude.Core.Implementation
{
	/// <summary>Simple paged list implementation.</summary>
	public class PagedList<T>: IPagedList<T>
	{
		private readonly IEnumerable<T> data;
		
		/// <constructor />
		public PagedList(int totalRowCount, int rowCount, IEnumerable<T> data)
		{
			TotalRowCount = totalRowCount;
			RowCount = rowCount;
			this.data = data;
		}

		/// <inheritdoc/>
		public int TotalRowCount { get; private set; }

		/// <inheritdoc/>
		public int RowCount { get; private set; }
		
		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			return data.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}