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
			this.settings = settings;
			this.couchApi = couchApi;
		}

		/// <inheritdoc/>
		public DocumentInfo Save<TEntity>(TEntity entity) where TEntity : new()
		{
			if(ReferenceEquals(entity, null)) throw new ArgumentNullException("entity");
			Contract.EndContractBlock();

			var documentEntity = DocumentEntity.FromEntity(entity, settings);

			if (cache.Contains(documentEntity))
				throw new ArgumentException("Instance is already in cache.", "entity");
			
			if(documentEntity.Revision != null)
				throw new ArgumentException("Saving entity should not contain revision.", "entity");

			if (documentEntity.Id == null)
				documentEntity.SetId(settings.IdGenerator.GenerateId());

			documentEntity.DoMap();
			var result = couchApi.SaveDocumentToDb(documentEntity.Id, documentEntity.Document);
			cache.Put(documentEntity);
			var documentInfo = CreateDocumentInfo(result);
			documentEntity.Revision = documentInfo.Revision;
			return documentInfo;
		}

		/// <summary>Deletes provided entity form CouchDB.</summary>
		public DocumentInfo Delete<TEntity>(TEntity entity) where TEntity : new()
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
		public TEntity Load<TEntity>(string docId) where TEntity : new()
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
			var documentEntity = DocumentEntity.FromJson<TEntity>(document, settings);
			cache.Put(documentEntity);

			return (TEntity) documentEntity.Entity;
		}

		/// <inheritdoc/>
		public TEntity Find<TEntity>(ViewInfo view) where TEntity : new()
		{
			throw new NotImplementedException();
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
			Flush();
			if (disposing)
				cache.Clear();
		}
	}
}