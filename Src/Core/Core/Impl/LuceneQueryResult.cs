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

namespace CouchDude.Impl
{
	/// <summary>CouchDB view query result class.</summary>
	public class LuceneQueryResult : QueryResult<LuceneResultRow>, ILuceneQueryResult
	{
		/// <summary>Empty query result.</summary>
		public static readonly ILuceneQueryResult Empty =
			new LuceneQueryResult(new LuceneQuery(), new LuceneResultRow[0], 0, 0, default(TimeSpan), default(TimeSpan), 0, 0);
		
		/// <constructor />
		public LuceneQueryResult(
			LuceneQuery query, ICollection<LuceneResultRow> rows, int totalCount, int offset, 
			TimeSpan fetchDuration, TimeSpan searchDuration, int limit, int skip)
			: base(rows, totalCount, offset) { Init(query, fetchDuration, searchDuration, limit, skip); }

		/// <constructor />
		public LuceneQueryResult(
			LuceneQuery query, IEnumerable<LuceneResultRow> rows, 
			int count, int totalCount, int offset, TimeSpan fetchDuration, TimeSpan searchDuration, int limit, int skip)
			: base(rows, count, totalCount, offset) { Init(query, fetchDuration, searchDuration, limit, skip); }

		private void Init(LuceneQuery query, TimeSpan fetchDuration, TimeSpan searchDuration, int limit, int skip)
		{
			Query = query;
			FetchDuration = fetchDuration;
			SearchDuration = searchDuration;
			Limit = limit;
			Skip = skip;
		}

		/// <inheritdoc/>
		public LuceneQuery Query { get; private set; }

		/// <inheritdoc/>
		public LuceneQuery NextPageQuery { get { return GetNextPageQuery(this); } }

		/// <inheritdoc/>
		public TimeSpan FetchDuration { get; private set; }

		/// <inheritdoc/>
		public TimeSpan SearchDuration { get; private set; }

		/// <inheritdoc/>
		public int Limit { get; private set; }

		/// <inheritdoc/>
		public int Skip { get; private set; }


		private static readonly ConcurrentDictionary<ILuceneQueryResult, LuceneQuery> NextPageQueries =
			new ConcurrentDictionary<ILuceneQueryResult, LuceneQuery>();
		internal static LuceneQuery GetNextPageQuery(ILuceneQueryResult queryResult)
		{
			return NextPageQueries.GetOrAdd(
				queryResult,
				r => {
					if (queryResult.Count == queryResult.TotalCount || queryResult.Count == 0)
						return null;
					else
					{
						var nextPageQuery = queryResult.Query.Clone();
						nextPageQuery.Skip = queryResult.Skip + queryResult.Count;
						nextPageQuery.Limit = queryResult.Limit;
						return nextPageQuery;
					}
				});
		}
	}

	/// <summary>Typed CouchDB view query result class.</summary>
	public class LuceneQueryResult<T> : QueryResult<T, LuceneResultRow>, ILuceneQueryResult<T>
	{
		/// <summary>Empty query result.</summary>
		// ReSharper disable StaticFieldInGenericType
		public static readonly ILuceneQueryResult<T> Empty =
			new LuceneQueryResult<T>(new LuceneQuery(), new LuceneResultRow[0], 0, 0, default(TimeSpan), default(TimeSpan), 0, 0, ToNullSequence);
		// ReSharper restore StaticFieldInGenericType

		private static IEnumerable<T> ToNullSequence(IEnumerable<LuceneResultRow> rows) 
		{
			return rows.Select(_ => default(T));
		}
		
		/// <constructor />
		public LuceneQueryResult(
			LuceneQuery query, ICollection<LuceneResultRow> rows, int totalCount, int offset,
			TimeSpan fetchDuration, TimeSpan searchDuration, int limit, int skip, Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> rowConvertor)
			: base(rows, totalCount, offset, rowConvertor) { Init(query, fetchDuration, searchDuration, limit, skip); }

		/// <constructor />
		public LuceneQueryResult(
			LuceneQuery query, IEnumerable<LuceneResultRow> rows,
			int count, int totalCount, int offset, TimeSpan fetchDuration, TimeSpan searchDuration, int limit, int skip,
			Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> rowConvertor)
			: base(rows, count, totalCount, offset, rowConvertor) { Init(query, fetchDuration, searchDuration, limit, skip); }

		private void Init(LuceneQuery query, TimeSpan fetchDuration, TimeSpan searchDuration, int limit, int skip)
		{
			Query = query;
			FetchDuration = fetchDuration;
			SearchDuration = searchDuration;
			Limit = limit;
			Skip = skip;
		}

		/// <inheritdoc/>
		public LuceneQuery Query { get; private set; }

		/// <inheritdoc/>
		public LuceneQuery NextPageQuery { get { return LuceneQueryResult.GetNextPageQuery(this); } }

		/// <inheritdoc/>
		public TimeSpan FetchDuration { get; private set; }

		/// <inheritdoc/>
		public TimeSpan SearchDuration { get; private set; }

		/// <inheritdoc/>
		public int Limit { get; private set; }

		/// <inheritdoc/>
		public int Skip { get; private set; }
	}
}