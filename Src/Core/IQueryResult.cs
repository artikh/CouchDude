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