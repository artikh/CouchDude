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
			var isEntityType = CheckIfEntityType<T>(query);
			WaitForFlushIfInProgress();

			return couchApi
				.QueryLucene(query)
				.ContinueWith(
					queryTask =>
					{
						var rawQueryResult = queryTask.Result;
						if (!isEntityType)
							return rawQueryResult.OfType(DeserializeViewData<T, LuceneResultRow>);

						UpdateUnitOfWork(rawQueryResult.Rows);
						return rawQueryResult.OfType(GetEntities<T, LuceneResultRow>);
					});
		}

		/// <inheritdoc/>
		public Task<IViewQueryResult<T>> Query<T>(ViewQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");
			if (query.Skip >= 10)
				throw new ArgumentException("View query should not use skip option greater then 9 (see http://tinyurl.com/couch-skip)", "query");
			var isEntityType = CheckIfEntityType<T>(query);
			WaitForFlushIfInProgress();

			return couchApi
				.Query(query)
				.ContinueWith(
					queryTask => {
						var rawQueryResult = queryTask.Result;
						if (!isEntityType) 
							return rawQueryResult.OfType(DeserializeViewData<T, ViewResultRow>);
						
						UpdateUnitOfWork(rawQueryResult.Rows);
						return rawQueryResult.OfType(GetEntities<T, ViewResultRow>);
					});
		}

		// ReSharper disable UnusedParameter.Local
		private bool CheckIfEntityType<T>(IQuery query)
		// ReSharper restore UnusedParameter.Local
		{
			var isEntityType = settings.TryGetConfig(typeof (T)) != null;
			if (isEntityType && !query.IncludeDocs)
				throw new QueryException("You should use IncludeDocs query option when querying for entities.");
			return isEntityType;
		}


		private static IEnumerable<T> DeserializeViewData<T, TRow>(IEnumerable<TRow> rows) where TRow : IQueryResultRow
		{
			return from row in rows
			       select row.Value into value
			       select value == null ? default(T) : (T) value.TryDeserialize(typeof (T));
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
						yield return (T) entity;
				}
				else
					yield return default(T);
			}
		}
	}
}
