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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Impl
{
	/// <summary>Session implementation.</summary>
	public partial class CouchSession
	{
		/// <inheritdoc/>
		public Task<ILuceneQueryResult<T>> QueryLucene<T>(LuceneQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");
			var isEntityType = IsEntityType<T>();
			if (isEntityType && !query.IncludeDocs)
				throw CreateShouldUseIncludeDocsException();
			WaitForFlushIfInProgress();

			using (SyncContext.SwitchToDefault())
				return QueryLuceneInternal<T>(query, isEntityType);
		}

		/// <inheritdoc/>
		public Task<IViewQueryResult<T>> Query<T>(ViewQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");
			if (query.Skip >= 10)
				throw new ArgumentException("View query should not use skip option greater then 9 (see http://tinyurl.com/couch-skip)", "query");
			var isEntityType = IsEntityType<T>();
			if (isEntityType && !query.IncludeDocs)
				throw CreateShouldUseIncludeDocsException();
			WaitForFlushIfInProgress();
			
			using (SyncContext.SwitchToDefault())
				return QueryInternal<T>(query, isEntityType);
		}

		async Task<ILuceneQueryResult<T>> QueryLuceneInternal<T>(LuceneQuery query, bool isEntityType)
		{
			var rawQueryResult = await databaseApi.QueryLucene(query);
			if (!isEntityType)
				return rawQueryResult.OfType(DeserializeViewData<T, LuceneResultRow>);

			UpdateUnitOfWork(rawQueryResult.Rows);
			return rawQueryResult.OfType(GetEntities<T, LuceneResultRow>);
		}

		async Task<IViewQueryResult<T>> QueryInternal<T>(ViewQuery query, bool isEntityType)
		{
			var rawQueryResult = await databaseApi.Query(query);
			if (!isEntityType)
				return rawQueryResult.OfType(DeserializeViewData<T, ViewResultRow>);

			UpdateUnitOfWork(rawQueryResult.Rows);
			return rawQueryResult.OfType(GetEntities<T, ViewResultRow>);
		}
		
		private static QueryException CreateShouldUseIncludeDocsException()
		{
			return new QueryException("You should use IncludeDocs query option when querying for entities.");
		}

		private bool IsEntityType<T>()
		{
			return settings.TryGetConfig(typeof (T)) != null;
		}


		private IEnumerable<T> DeserializeViewData<T, TRow>(IEnumerable<TRow> rows) where TRow : IQueryResultRow
		{
			return from row in rows
			       select row.Value into value
			       select value == null ? default(T) : settings.Serializer.ConvertFromJson<T>(value, throwOnError: false);
		}

		private void UpdateUnitOfWork<TRow>(IEnumerable<TRow> rows) where TRow: IQueryResultRow
		{
			var documentsToUpdateUnitOfWorkWith =
				from row in rows
				let documentId = row.DocumentId
				where documentId.HasValue()
				select row.Document into document
				where document != null
				select document;

			lock (unitOfWork)
				foreach (var document in documentsToUpdateUnitOfWorkWith)
					unitOfWork.UpdateWithDocument(document);
		}

		private IEnumerable<T> GetEntities<T, TRow>(IEnumerable<TRow> rows) where TRow : IQueryResultRow
		{
			lock (unitOfWork)
				return GetEntitiesInt<T, TRow>(rows).ToArray();
		}

		private IEnumerable<T> GetEntitiesInt<T, TRow>(IEnumerable<TRow> rows) where TRow : IQueryResultRow
		{
			foreach (var documentId in rows.Select(row => row.DocumentId)) 
			{
				if (documentId.HasValue())
				{
					object entity;
					if (unitOfWork.TryGetByDocumentId(documentId, out entity) && entity is T)
						yield return (T)entity;
					else
						yield return default(T);
				}
				else
					yield return default(T);
			}
		}
	}
}
