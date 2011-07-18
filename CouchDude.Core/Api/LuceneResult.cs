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
	public class LuceneResult: IPagedList<LuceneResultRow>
	{
		/// <summary>Empty view result list.</summary>
		public static readonly LuceneResult Empty = new LuceneResult(new List<LuceneResultRow>(), 0, 0, null);

		private readonly ICollection<LuceneResultRow> rows;
		
		/// <constructor />
		public LuceneResult(ICollection<LuceneResultRow> rows, int totalRows, int offset, LuceneQuery query)
		{
			this.rows = rows;
			Offset = offset;
			TotalRowCount = totalRows;
			Query = query;
		}

		/// <inheritdoc/>
		public LuceneQuery Query { get; private set; }

		/// <inheritdoc/>
		public int TotalRowCount { get; private set; }

		/// <inheritdoc/>
		public int RowCount { get { return rows.Count; } }

		/// <inheritdoc/>
		public int Offset { get; private set; }

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc/>
		public IEnumerator<LuceneResultRow> GetEnumerator()
		{
			return rows.GetEnumerator();
		}
	}
}
