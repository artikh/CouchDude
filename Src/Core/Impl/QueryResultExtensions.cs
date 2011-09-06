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
using System.Collections.Generic;

namespace CouchDude.Impl
{
	/// <summary>Extension methods for <see cref="IQueryResult{T,TRow}"/> descendants.</summary>
	public static class QueryResultExtensions
	{
		/// <summary>Converts untyped query result ot typed one.</summary>
		public static ILuceneQueryResult<T> OfType<T>(this ILuceneQueryResult result, Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> rowConvertor)
		{
			return new LuceneQueryResult<T>(
				result.Query, result.Rows, result.Count, result.TotalCount, result.Offset, 
				result.FetchDuration, result.SearchDuration, result.Limit, result.Offset,  rowConvertor);
		}

		/// <summary>Converts untyped query result ot typed one.</summary>
		public static IViewQueryResult<T> OfType<T>(this IViewQueryResult result, Func<IEnumerable<ViewResultRow>, IEnumerable<T>> rowConvertor)
		{
			return new ViewQueryResult<T>(result.Query, result.Rows, result.Count, result.TotalCount, result.Offset, rowConvertor);
		}
	}
}