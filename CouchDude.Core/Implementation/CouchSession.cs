using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Implementation
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
					throw new EntityTypeMismatchException(documentEntity.EntityType, typeof (TEntity));
			}
			else
				documentEntity = DocumentEntity.FromEntity(entity, settings);

			if (documentEntity.Revision == null)
				throw new ArgumentException(
					"No revision property found on entity and no revision information" 
						+ " found in first level cache.", 
					"entity");

			cache.Remove(documentEntity);

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

			var cachedEntity = cache.TryGet(entityId);
			if (cachedEntity != null)
			{
				if (!typeof (TEntity).IsAssignableFrom(cachedEntity.EntityType))
					throw new EntityTypeMismatchException(cachedEntity.EntityType, typeof (TEntity));
				return (TEntity) cachedEntity.Entity;
			}

			var documentType = settings.TypeConvension.GetDocumentType(typeof (TEntity));
			if (documentType == null)
				throw new ConfigurationException("Type {0} have not been registred.", typeof(TEntity));
			var docId = documentType + "." + entityId;

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
			foreach (var documentEntity 
				in cache.DocumentEntities.Where(documentEntity => documentEntity.CheckIfChanged()))
			{
				documentEntity.DoMap();
				couchApi.UpdateDocumentInDb(documentEntity.DocumentId, documentEntity.Document);
			}
		}

		/// <summary>Backup plan finalizer - use Dispose() method!</summary>
		~CouchSession()
		{
			Dispose(disposing: false);
		}

		/// <inheritdoc/>
		public IPagedList<T> Query<T>(ViewQuery<T> query) where T : class
		{
			if (query == null) throw new ArgumentNullException("query");
			var isEntityType = settings.TypeConvension.GetDocumentType(typeof (T)) != null;
			if (isEntityType && !query.IncludeDocs)
				throw new ArgumentException("You should use IncludeDocs query option when querying entities.");
			Contract.EndContractBlock();

			var queryResult = couchApi.Query(query);
			return isEntityType ? GetEntityList<T>(queryResult) : GetViewDataList<T>(queryResult);
		}

		private IPagedList<T> GetEntityList<T>(ViewResult queryResult) where T : class
		{
			var entities = (
				from row in queryResult.Rows
				where row.Document != null
				let documentEntity = DocumentEntity.FromJson<T>(row.Document, settings, throwOnTypeMismatch: false)
				where documentEntity != null
				select cache.PutOrReplace(documentEntity)
			).ToArray();

			return new PagedList<T>(
				queryResult.TotalRows, entities.Length, entities.Select(de => (T)de.Entity));
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