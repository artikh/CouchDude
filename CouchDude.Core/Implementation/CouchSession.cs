using System;
using System.Collections.Generic;
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
			var result = couchApi.SaveDocumentToDb(documentEntity.Id, documentEntity.Document);
			cache.Put(documentEntity);
			var documentInfo = CreateDocumentInfo(result);
			documentEntity.Revision = documentInfo.Revision;
			return documentInfo;
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

			return CreateDocumentInfo(
				couchApi.DeleteDocument(documentEntity.Id, documentEntity.Revision));
		}

		/// <inheritdoc/>
		public TEntity Load<TEntity>(string docId) where TEntity : class
		{
			if (string.IsNullOrWhiteSpace(docId)) 
				throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var cachedEntity = cache.TryGet(docId);
			if (cachedEntity != null)
			{
				if (!typeof (TEntity).IsAssignableFrom(cachedEntity.EntityType))
					throw new EntityTypeMismatchException(cachedEntity.EntityType, typeof (TEntity));
				return (TEntity) cachedEntity.Entity;
			}

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

		private static DocumentInfo CreateDocumentInfo(JObject result)
		{
			return new DocumentInfo(
				result.GetRequiredProperty("id"), result.GetRequiredProperty("rev"));
		}

		/// <inheritdoc/>
		public void Flush()
		{
			foreach (var documentEntity 
				in cache.DocumentEntities.Where(documentEntity => documentEntity.CheckIfChanged()))
			{
				documentEntity.DoMap();
				couchApi.UpdateDocumentInDb(documentEntity.Id, documentEntity.Document);
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