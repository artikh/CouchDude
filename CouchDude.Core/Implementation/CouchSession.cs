using System;
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

			documentEntity.DoMap();
			var result = couchApi.SaveDocumentToDb(documentEntity.Id, documentEntity.Document);
			cache.Put(documentEntity);
			var documentInfo = new DocumentInfo(
				result.GetRequiredProperty("id"), result.GetRequiredProperty("rev"));
			documentEntity.Revision = documentInfo.Revision;
			return documentInfo;
		}

		/// <inheritdoc/>
		public TEntity Load<TEntity>(string docId) where TEntity : new()
		{
			if (string.IsNullOrWhiteSpace(docId)) 
				throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var cachedEntity = cache.TryGet(docId);
			if (cachedEntity != null)
				if (typeof (TEntity).IsAssignableFrom(cachedEntity.EntityType)) 
					return (TEntity) cachedEntity.Entity;
				else
					throw new EntityTypeMismatchException(cachedEntity.EntityType, typeof(TEntity));

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