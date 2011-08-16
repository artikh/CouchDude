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
using System.Linq;

namespace CouchDude.Impl
{
	/// <summary>Simple paged list implementation.</summary>
	public class PagedList<T>: IPagedList<T>
	{	
		// ReSharper disable StaticFieldInGenericType
		/// <summary>Empty paged list of given type.</summary>
		public static PagedList<T> Empty = new PagedList<T>(new T[0], 0, 0);
		// ReSharper restore StaticFieldInGenericType

		private readonly IEnumerable<T> data;
		private int? rowCount;
		
		/// <constructor />
		public PagedList(IEnumerable<T> data, int totalRowCount, int offset)
		{
			TotalRowCount = totalRowCount;
			Offset = offset;
			this.data = data;
		}

		/// <inheritdoc/>
		public int TotalRowCount { get; private set; }

		/// <inheritdoc/>
		public int RowCount { get { return rowCount ?? (rowCount = data.Count()).Value; } }

		/// <inheritdoc/>
		public int Offset { get; private set; }

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