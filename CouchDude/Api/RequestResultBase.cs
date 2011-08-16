#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
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
using System.Linq;

namespace CouchDude.Api
{
	/// <summary>Base class for <see cref="ViewQuery"/> and <see cref="LuceneResult"/>.</summary>
	public class RequestResultBase<TRow, TQuery, TSelf> : IPagedList<TRow> 
		where TSelf: RequestResultBase<TRow, TQuery, TSelf>
		where TQuery: class
	{
		
		/// <summary>Empty view result list.</summary>
		// ReSharper disable StaticFieldInGenericType
		public static readonly TSelf Empty = (TSelf)new RequestResultBase<TRow, TQuery, TSelf>(new List<TRow>(), 0, 0, null);
		// ReSharper restore StaticFieldInGenericType

		private readonly IEnumerable<TRow> rows;
		private int? rowCount;
		
		/// <constructor />
		public RequestResultBase(IEnumerable<TRow> rows, int totalRowCount, int offset, TQuery query)
		{
			this.rows = rows;
			Offset = offset;
			TotalRowCount = totalRowCount;
			Query = query;
		}

		/// <inheritdoc/>
		public TQuery Query { get; private set; }

		/// <inheritdoc/>
		public int TotalRowCount { get; private set; }

		/// <inheritdoc/>
		public int RowCount { get { return rowCount ?? (rowCount = rows.Count()).Value; } }

		/// <inheritdoc/>
		public int Offset { get; private set; }
		
		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc/>
		public IEnumerator<TRow> GetEnumerator() { return rows.GetEnumerator(); }
	}
}