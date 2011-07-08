using System;
using System.Diagnostics.Contracts;
using System.Linq;
using CouchDude.Core.Api;
using CouchDude.Core.Utils;
using Newtonsoft.Json.Linq;

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
			var documentEntity = DocumentEntity.FromJson<TEntity>(document, settings);
			cache.Put(documentEntity);

			return (TEntity) documentEntity.Entity;
		}

		/// <inheritdoc/>
		public void Flush()
		{
			foreach (var documentEntity in cache.DocumentEntities.Where(documentEntity => documentEntity.CheckIfChanged()))
			{
				documentEntity.DoMap();
				couchApi.UpdateDocumentInDb(documentEntity.DocumentId, documentEntity.Document);
			}
		}

		/// <inheritdoc/>
		public IPagedList<T> Query<T>(ViewQuery<T> query) where T : class
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
			return isEntityType ? GetEntityList<T>(queryResult) : GetLuceneViewDataList<T>(queryResult);
		}

		// ReSharper disable UnusedParameter.Local
		[Pure]
		private bool CheckIfEntityType<T>(bool includeDocs) where T : class
		// ReSharper restore UnusedParameter.Local
		{
			var isEntityType = settings.TryGetConfig(typeof (T)) != null;
			if (isEntityType && !includeDocs)
				throw new ArgumentException("You should use IncludeDocs query option when querying entities.");
			return isEntityType;
		}

		private static IPagedList<T> GetLuceneViewDataList<T>(LuceneResult queryResult) where T : class
		{
			var viewDataList = (
				from row in queryResult.Rows.AsParallel()
				where row.Fields != null				
				let viewDataItem = DeserializeViewData<T>(row.Fields) 
				select viewDataItem
			).ToArray();
			return new PagedList<T>(queryResult.TotalRows, viewDataList.Length, viewDataList);
		}


		private IPagedList<T> GetEntityList<T>(ViewResult queryResult) where T : class 
		{
			var entities = (
				from row in queryResult.Rows
				where row.Document != null
				let documentEntity = DocumentEntity.TryFromJson<T>(row.Document, settings)
				where documentEntity != null
				select cache.PutOrReplace(documentEntity)
			).ToArray();

			return new PagedList<T>(queryResult.TotalRows, entities.Length, entities.Select(de => (T)de.Entity));
		}

		private IPagedList<T> GetEntityList<T>(LuceneResult queryResult) where T : class
		{
			var entities = (
				from row in queryResult.Rows
				where row.Document != null
				let documentEntity = DocumentEntity.TryFromJson<T>(row.Document, settings)
				where documentEntity != null
				select cache.PutOrReplace(documentEntity)
			).ToArray();

			return new PagedList<T>(queryResult.TotalRows, entities.Length, entities.Select(de => (T)de.Entity));
		}

		private static IPagedList<T> GetViewDataList<T>(ViewResult queryResult) where T : class
		{
			var viewDataList = (
				from row in queryResult.Rows.AsParallel()
				where row.Value != null
				let viewDataItem = DeserializeViewData<T>(row.Value)
				select viewDataItem
			).ToArray();
			return new PagedList<T>(queryResult.TotalRows, viewDataList.Length, viewDataList);
		}

		private static T DeserializeViewData<T>(JToken value) where T : class
		{
			using (var reader = new JTokenReader(value))
				return JsonSerializer.Instance.Deserialize<T>(reader);
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