using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

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

			var documentType = settings.GetDocumentType<TEntity>();
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
		public IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class
		{
			var allDocuments = couchApi.Query(new ViewQuery {
				DesignDocumentName = null,
				ViewName = "_all_docs",
				IncludeDocs = true
			});
			
			foreach (var resultRow in allDocuments.Rows)
			{
				var documentEntity = DocumentEntity.FromJson<TEntity>(resultRow.Document, settings, throwOnTypeMismatch: false);
				if (documentEntity != null)
				{
					cache.Put(documentEntity);
					yield return documentEntity.GetEntity<TEntity>();
				}
			}
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