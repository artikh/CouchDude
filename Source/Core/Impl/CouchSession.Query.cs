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
					queryTask => {
						var rawResult = queryTask.Result;
						if (isEntityType)
							lock (unitOfWork)
							{
								var result = rawResult.OfType(DeserializeEntity<T, LuceneResultRow>);
								// ReSharper disable ReturnValueOfPureMethodIsNotUsed
								result.GetEnumerator();
								// forcing collection to fill out cache so it happans inside of unit of work lock
								// ReSharper restore ReturnValueOfPureMethodIsNotUsed
								return result;
							}
						else
							return rawResult.OfType(DeserializeViewData<T, LuceneResultRow>);
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
						var rawResult = queryTask.Result;
						if (isEntityType)
							lock (unitOfWork)
							{
								var result = rawResult.OfType(DeserializeEntity<T, ViewResultRow>);
								// ReSharper disable ReturnValueOfPureMethodIsNotUsed
								result.GetEnumerator();
								// forcing collection to fill out cache so it happans inside of unit of work lock
								// ReSharper restore ReturnValueOfPureMethodIsNotUsed
								return result;
							}
						else
							return rawResult.OfType(DeserializeViewData<T, ViewResultRow>);
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

		private static T DeserializeViewData<T, TRow>(TRow row) where TRow : IQueryResultRow
		{
			var value = row.Value;
			return value == null ? default(T) : (T)value.TryDeserialize(typeof(T));
		}
		
		private T DeserializeEntity<T, TRow>(TRow row) where TRow : IQueryResultRow
		{
			var documentId = row.DocumentId;
			if (documentId.HasValue())
			{
				var document = row.Document;
				if (document != null)
					unitOfWork.UpdateWithDocument(document);
				object entity;
				if (unitOfWork.TryGetByDocumentId(documentId, out entity) && entity is T)
					return (T) entity;
			}
			
			return default(T);
		}
	}
}
