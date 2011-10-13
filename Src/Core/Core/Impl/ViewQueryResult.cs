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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CouchDude.Utils;

namespace CouchDude.Impl
{
	/// <summary>CouchDB view query result class.</summary>
	public class ViewQueryResult: QueryResult<ViewResultRow>, IViewQueryResult
	{
		/// <summary>Empty query result.</summary>
		public static readonly IViewQueryResult Empty = 
			new ViewQueryResult(query: new ViewQuery(), rows: new ViewResultRow[0], totalCount: 0, offset: 0);

		/// <constructor />
		public ViewQueryResult(ViewQuery query, ICollection<ViewResultRow> rows, int totalCount, int offset) 
			: base(rows, totalCount, offset) { Query = query; }

		/// <constructor />
		public ViewQueryResult(ViewQuery query, IEnumerable<ViewResultRow> rows, int count, int totalCount, int offset) 
			: base(rows, count, totalCount, offset) { Query = query; }

		/// <inheritdoc/>
		public ViewQuery Query { get; private set; }

		/// <inheritdoc/>
		public ViewQuery NextPageQuery { get { return GetNextPageQuery(this); } }
		
		private static readonly ConcurrentDictionary<IViewQueryResult, ViewQuery> NextPageQueries =
			new ConcurrentDictionary<IViewQueryResult, ViewQuery>();
		internal static ViewQuery GetNextPageQuery(IViewQueryResult queryResult)
		{
			return NextPageQueries.GetOrAdd(
				queryResult,
				r =>
				{
					if (queryResult.Count == queryResult.TotalCount || queryResult.Count == 0)
						return null;
					else
					{
						var nextPageQuery = queryResult.Query.Clone();
						var lastItem = queryResult.Rows.Last();
						nextPageQuery.Skip = 1;

						if (nextPageQuery.StartKey != null)
							nextPageQuery.StartKey = lastItem.Key;

						if (lastItem.DocumentId.HasValue())
							nextPageQuery.StartDocumentId = lastItem.DocumentId;
						return nextPageQuery;
					}
				});
		}
	}

	/// <summary>Typed CouchDB view query result class.</summary>
	public class ViewQueryResult<T> : QueryResult<T, ViewResultRow>, IViewQueryResult<T>
	{
		/// <summary>Empty query result.</summary>
		// ReSharper disable StaticFieldInGenericType
		public static readonly IViewQueryResult<T> Empty =
			new ViewQueryResult<T>(query: new ViewQuery(), rows: new ViewResultRow[0], totalCount: 0, offset: 0, rowConvertor: rows => rows.Select(_ => default(T)));
		// ReSharper restore StaticFieldInGenericType

		/// <constructor />
		public ViewQueryResult(
			ViewQuery query, ICollection<ViewResultRow> rows, int totalCount, int offset, Func<IEnumerable<ViewResultRow>, IEnumerable<T>> rowConvertor)
			: base(rows, totalCount, offset, rowConvertor) { Query = query; }

		/// <constructor />
		public ViewQueryResult(
			ViewQuery query, IEnumerable<ViewResultRow> rows, int count, int totalCount, int offset, Func<IEnumerable<ViewResultRow>, IEnumerable<T>> rowConvertor)
			: base(rows, count, totalCount, offset, rowConvertor) { Query = query; }

		/// <inheritdoc/>
		public ViewQuery Query { get; private set; }

		/// <inheritdoc/>
		public ViewQuery NextPageQuery { get { return ViewQueryResult.GetNextPageQuery(this); } }
	}
}