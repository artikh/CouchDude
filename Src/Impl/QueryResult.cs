#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

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
		private readonly Func<IEnumerable<TRow>, IEnumerable<T>> rowConvertor;
		private readonly object syncHandle = new object();
		private volatile IEnumerable<T> convertedRows;

		/// <constructor />
		protected QueryResult(ICollection<TRow> rows, int? totalCount, int? offset, Func<IEnumerable<TRow>, IEnumerable<T>> rowConvertor)
			: base(rows, totalCount, offset)
		{
			this.rowConvertor = rowConvertor;
		}

		/// <constructor />
		protected QueryResult(IEnumerable<TRow> rows, int count, int? totalCount, int? offset, Func<IEnumerable<TRow>, IEnumerable<T>> rowConvertor)
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
						convertedRows = rowConvertor(Rows);
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
		private readonly int? totalCount;
		private readonly int? offset;

		/// <constructor />
		protected QueryResult(ICollection<TRow> rows, int? totalCount, int? offset)
			: this(rows, rows.Count, totalCount, offset) { }

		/// <constructor />
		protected QueryResult(IEnumerable<TRow> rows, int count, int? totalCount, int? offset)
		{
			if (offset.HasValue && offset.Value < 0) 
				throw new ArgumentOutOfRangeException("offset", offset, "Offset should be positive number or null.");
			if (totalCount.HasValue && totalCount.Value < 0) 
				throw new ArgumentOutOfRangeException("totalCount", offset, "Total count should be positive number or null.");
			if (count < 0) throw new ArgumentOutOfRangeException("count", offset, "Count should be positive number.");
			if (rows == null) throw new ArgumentNullException("rows");

			this.rows = new ReadOnlyCollection<TRow>(rows.ToList());
			this.offset = offset;
			this.count = count;
			this.totalCount = totalCount;
		}

		/// <inheritdoc/>
		public int Count { get { return count; } }
		/// <inheritdoc/>
		public int? TotalCount { get { return totalCount; } }
		/// <inheritdoc/>
		public int? Offset { get { return offset; } }
		/// <inheritdoc/>
		public IEnumerable<TRow> Rows { get { return rows; } }
	}
}