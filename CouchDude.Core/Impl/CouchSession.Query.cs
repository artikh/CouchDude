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
using System.Linq;
using System.Threading.Tasks;

namespace CouchDude.Core.Impl
{
	/// <summary>Session implementation.</summary>
	public partial class CouchSession
	{
		/// <inheritdoc/>
		public Task<IPagedList<T>> FulltextQuery<T>(LuceneQuery<T> query) where T : class
		{
			if (query == null)
				throw new ArgumentNullException("query");

			return QueryInternal<T, LuceneQuery<T>, LuceneResultRow>(query, (api, q) => api.QueryLucene(q));
		}

		/// <inheritdoc/>
		public Task<IPagedList<T>> Query<T>(ViewQuery<T> query)
		{
			if (query == null)
				throw new ArgumentNullException("query");
			if (query.Skip >= 10)
				throw new ArgumentException("", "query");

			return QueryInternal<T, ViewQuery<T>, ViewResultRow>(query, (api, q) => api.Query(q));
		}

		private Task<IPagedList<T>> QueryInternal<T, TQuery, TRow>(
			TQuery query, Func<ICouchApi, TQuery, Task<IPagedList<TRow>>> queryTask)
			where TQuery : IQuery<TRow, T>
			where TRow : IQueryResultRow
		{
			var isEntityType = settings.TryGetConfig(typeof(T)) != null;
			if (isEntityType && !query.IncludeDocs)
				throw new QueryException("You should use IncludeDocs query option when querying for entities.");

			return queryTask(couchApi, query).ContinueWith<IPagedList<T>>(
				qt =>{
					var rawQueryResult = qt.Result;
					IEnumerable<T> queryResultRows;
					if (query.ProcessRows != null)
						queryResultRows = query.ProcessRows(rawQueryResult);
					else if (isEntityType)
						queryResultRows = DeserializeEntitiesAndCache<T, TRow>(rawQueryResult);
					else
						queryResultRows = DeserializeViewData<T, TRow>(rawQueryResult);

					return new PagedList<T>(queryResultRows, rawQueryResult.TotalRowCount, rawQueryResult.Offset);
				}
			);
		}

		private static IEnumerable<T> DeserializeViewData<T, TRow>(IEnumerable<TRow> rawViewResults) where TRow : IQueryResultRow
		{
			return from row in rawViewResults
						 select row.Value into value
						 select value == null ? default(T) : (T)value.TryDeserialize(typeof(T));
		}

		private IEnumerable<T> DeserializeEntitiesAndCache<T, TRow>(IEnumerable<TRow> queryResult) where TRow : IQueryResultRow
		{
			var documentEntities =
				queryResult.Select(row => DocumentEntity.TryFromDocument<T>(row.Document, settings)).ToArray();

			foreach (var documentEntity in documentEntities)
				if (documentEntity != null)
					cache.PutOrReplace(documentEntity);

			return from documentEntity in documentEntities
						 select documentEntity == null ? default(T) : (T)documentEntity.Entity;
		}
	}
}
