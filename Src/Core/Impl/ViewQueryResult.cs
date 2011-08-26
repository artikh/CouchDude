using System.Collections.Generic;

namespace CouchDude.Impl
{
	/// <summary>Standard <see cref="IViewQueryResult{T}"/> implementation</summary>
	public class ViewQueryResult<T> : PagedList<T>, IViewQueryResult<T>
	{
		/// <constructor />
		public ViewQueryResult(
			IEnumerable<T> data, int totalRowCount, int offset, ViewQuery query) 
			: base(data, totalRowCount, offset) { Query = query; }

		/// <inheritdoc/>
		public ViewQuery Query { get; private set; }

		/// <inheritdoc/>
		public ViewQuery GetNextPageQuery() { return null; }
	}
}