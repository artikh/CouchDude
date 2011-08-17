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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Impl
{
	/// <summary>Session implementation.</summary>
	public partial class CouchSession : ISession
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
			Synchronously = new SynchronousSessionMethods(this);
		}

		/// <inheritdoc/>
		public ISynchronousSessionMethods Synchronously { get; private set; }

		/// <inheritdoc/>
		public ICouchApi RawApi { get { return couchApi; } }

		/// <inheritdoc/>
		public void Save<TEntity>(TEntity entity) where TEntity : class
		{
			if(ReferenceEquals(entity, null)) throw new ArgumentNullException("entity");
			Contract.EndContractBlock();

			var documentEntity = DocumentEntity.FromEntity(entity, settings);

			if (cache.Contains(documentEntity))
				throw new ArgumentException("Instance is already in cache.", "entity");
			
			if(documentEntity.Revision != null)
				throw new ArgumentException("Saving entity should not contain revision.", "entity");
			
			documentEntity.DoMap();

			// TODO: Should write to the unit of work insted of DB
			dynamic documentInfo = couchApi.Synchronously.SaveDocumentSync(documentEntity.Document);
			cache.Put(documentEntity);

			documentEntity.Revision = documentInfo.rev;
		}

		/// <summary>Deletes provided entity form CouchDB.</summary>
		public void Delete<TEntity>(TEntity entity) where TEntity : class
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

			// TODO: Should delete from the unit of work insted of DB
			couchApi.Synchronously.DeleteDocument(documentEntity.DocumentId, documentEntity.Revision);
			cache.Remove(documentEntity);
		}

		/// <inheritdoc/>
		public Task<TEntity> Load<TEntity>(string entityId) where TEntity : class
		{
			if (string.IsNullOrWhiteSpace(entityId)) 
				throw new ArgumentNullException("entityId");
			Contract.EndContractBlock();

			var cachedEntity = cache.TryGet(entityId, typeof(TEntity));
			if (cachedEntity != null)
			{
				if (!typeof (TEntity).IsAssignableFrom(cachedEntity.EntityType))
					throw new EntityTypeMismatchException(cachedEntity.EntityType, typeof (TEntity));
				return Task.Factory.StartNew(() => (TEntity) cachedEntity.Entity);
			}

			var entityConfig = settings.GetConfig(typeof (TEntity));
			if (entityConfig == null)
				throw new EntityTypeNotRegistredException(typeof(TEntity));
			var docId = entityConfig.ConvertEntityIdToDocumentId(entityId);

			return couchApi.RequestDocumentById(docId).ContinueWith(rt => {
				var document = rt.Result;
				if (document == null)
					return null;
				var documentEntity = DocumentEntity.FromDocument<TEntity>(document, settings);
				cache.Put(documentEntity);

				return (TEntity)documentEntity.Entity;                                                		
			});
		}

		/// <inheritdoc/>
		public Task StartSavingChanges()
		{
			var saveTasks = new List<Task>();
			foreach (var de in cache.DocumentEntities.Where(documentEntity => documentEntity.CheckIfChanged()))
			{
				var documentEntity = de;
				documentEntity.DoMap();
				var updateTask = couchApi
					.UpdateDocument(documentEntity.Document)
					.ContinueWith(pt => {
						dynamic documentInfo = pt.Result;
					  documentEntity.Revision = (string) documentInfo.rev;
					});

				saveTasks.Add(updateTask);
			}
			return Task.Factory.ContinueWhenAll(saveTasks.ToArray(), tasks => { });
		}

		/// <inheritdoc/>
		public void SaveChanges()
		{
			StartSavingChanges().WaitForResult();
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