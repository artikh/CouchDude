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

using System.Collections;
using System.Collections.Generic;

namespace CouchDude.Core.Api
{
	/// <summary>Result of CouchDB view query.</summary>
	public class ViewResult : IPagedList<ViewResultRow>
	{

		private readonly ICollection<ViewResultRow> rows;

		/// <constructor />
		public ViewResult() : this(new List<ViewResultRow>(), 0, null) { }

		/// <constructor />
		public ViewResult(ICollection<ViewResultRow> rows, int totalRows, ViewQuery query)
		{
			this.rows = rows;
			TotalRowCount = totalRows;
			Query = query;
		}

		/// <summary>Query used to produce result.</summary>
		public ViewQuery Query { get; private set; }

		/// <summary>Total rows in requested range.</summary>
		public int TotalRowCount { get; private set; }

		/// <summary>Rows selected.</summary>
		public int RowCount { get { return rows.Count; } }

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc/>
		public IEnumerator<ViewResultRow> GetEnumerator()
		{
			return rows.GetEnumerator();
		}
	}
}
