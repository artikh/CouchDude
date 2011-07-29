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
using System.Diagnostics.Contracts;
using System.Linq;

namespace CouchDude.Core.Impl
{
	/// <summary>Session implementation.</summary>
	public class CouchSession: ISession
	{
		private readonly Settings settings;
		private readonly ICouchApi couchApi;
		private readonly DocumentEntityCache cache = new DocumentEntityCache();

		/// <constructor />
		public CouchSession(Settings settings, ICouchApi couchApi)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (settings.Incomplete) throw new ArgumentException("Settings are incomplete.", "settings");
			if (couchApi == null) throw new ArgumentNullException("couchApi");
			Contract.EndContractBlock();

			this.settings = settings;
			this.couchApi = couchApi;
		}

		/// <inheritdoc/>
		public DocumentInfo Save<TEntity>(TEntity entity) where TEntity : class
		{
			if(ReferenceEquals(entity, null)) throw new ArgumentNullException("entity");
			Contract.EndContractBlock();

			var documentEntity = DocumentEntity.FromEntity(entity, settings);

			if (cache.Contains(documentEntity))
				throw new ArgumentException("Instance is already in cache.", "entity");
			
			if(documentEntity.Revision != null)
				throw new ArgumentException("Saving entity should not contain revision.", "entity");
			
			documentEntity.DoMap();
			var result = couchApi.SaveDocumentToDb(documentEntity.DocumentId, documentEntity.Document);
			cache.Put(documentEntity);

			var newRevision = result.GetRequiredProperty("rev");
			documentEntity.Revision = newRevision;
			return new DocumentInfo(documentEntity.EntityId, newRevision);
		}

		/// <summary>Deletes provided entity form CouchDB.</summary>
		public DocumentInfo Delete<TEntity>(TEntity entity) where TEntity : class
		{
			var documentEntity = cache.TryGet(entity);
			if (documentEntity != null)
			{
				if (!typeof (TEntity).IsAssignableFrom(documentEntity.EntityType))
					throw new EntityTypeMismatchException(documentEntity.EntityType, typeof(TEntity));
				cache.Remove(documentEntity);
			}
			else
				documentEntity = DocumentEntity.FromEntity(entity, settings);

			if (documentEntity.Revision == null)
				throw new ArgumentException(
					"No revision property found on entity and no revision information" 
						+ " found in first level cache.", 
					"entity");
			
			var result = couchApi.DeleteDocument(documentEntity.DocumentId, documentEntity.Revision);
			var newRevision = result.GetRequiredProperty("rev");
			return new DocumentInfo(documentEntity.EntityId, newRevision);
		}

		/// <inheritdoc/>
		public TEntity Load<TEntity>(string entityId) where TEntity : class
		{
			if (string.IsNullOrWhiteSpace(entityId)) 
				throw new ArgumentNullException("entityId");
			Contract.EndContractBlock();

			var cachedEntity = cache.TryGet(entityId, typeof(TEntity));
			if (cachedEntity != null)
			{
				if (!typeof (TEntity).IsAssignableFrom(cachedEntity.EntityType))
					throw new EntityTypeMismatchException(cachedEntity.EntityType, typeof (TEntity));
				return (TEntity) cachedEntity.Entity;
			}

			var entityConfig = settings.GetConfig(typeof (TEntity));
			if (entityConfig == null)
				throw new EntityTypeNotRegistredException(typeof(TEntity));
			var docId = entityConfig.ConvertEntityIdToDocumentId(entityId);

			var document = couchApi.GetDocumentFromDbById(docId);
			if (document == null)
				return null;
			var documentEntity = DocumentEntity.FromDocument<TEntity>(document, settings);
			cache.Put(documentEntity);

			return (TEntity) documentEntity.Entity;
		}

		/// <inheritdoc/>
		public void SaveChanges()
		{
			foreach (var documentEntity in cache.DocumentEntities.Where(documentEntity => documentEntity.CheckIfChanged()))
			{
				documentEntity.DoMap();
				couchApi.UpdateDocumentInDb(documentEntity.DocumentId, documentEntity.Document);
			}
		}

		/// <inheritdoc/>
		public IPagedList<T> Query<T>(ViewQuery<T> query)
		{
			if (query == null) 
				throw new ArgumentNullException("query");

			var isEntityType = CheckIfEntityType<T>(query.IncludeDocs);

			var queryResult = couchApi.Query(query);
			return isEntityType ? GetEntityList<T>(queryResult) : GetViewDataList<T>(queryResult);
		}

		/// <inheritdoc/>
		public IPagedList<T> FulltextQuery<T>(LuceneQuery<T> query) where T : class
		{
			if (query == null)
				throw new ArgumentNullException("query");
			var isEntityType = CheckIfEntityType<T>(query.IncludeDocs);
			Contract.EndContractBlock();

			var queryResult = couchApi.FulltextQuery(query);
			
			return isEntityType ? GetEntityList<T>(queryResult) : GetViewDataList<T>(queryResult);
		}

		// ReSharper disable UnusedParameter.Local
		[Pure]
		private bool CheckIfEntityType<T>(bool includeDocs)
		// ReSharper restore UnusedParameter.Local
		{
			var isEntityType = settings.TryGetConfig(typeof (T)) != null;
			if (isEntityType && !includeDocs)
				throw new ArgumentException("You should use IncludeDocs query option when querying entities.");
			return isEntityType;
		}
		
		private IPagedList<T> GetEntityList<T>(IPagedList<ViewResultRow> queryResult)
		{
			var documentEntities =
				queryResult.Select(row => DocumentEntity.TryFromDocument<T>(row.Document, settings)).ToArray();
			
			foreach (var documentEntity in documentEntities)
				if (documentEntity != null)
					cache.PutOrReplace(documentEntity);

			var entities = from de in documentEntities select de == null ? default(T) : (T) de.Entity;

			return new PagedList<T>(entities, queryResult.TotalRowCount, queryResult.Offset);
		}

		private static IPagedList<T> GetViewDataList<T>(IPagedList<ViewResultRow> queryResult)
		{
			var viewDataList =
				from row in queryResult select row.Value != null ? (T)row.Value.Deserialize(typeof(T)) : default(T);

			return new PagedList<T>(viewDataList, queryResult.TotalRowCount, queryResult.Offset);
		}
		
		/// <summary>Backup plan finalizer - use Dispose() method!</summary>
		~CouchSession()
		{
			Dispose(disposing: false);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
				cache.Clear();
		}
	}
}