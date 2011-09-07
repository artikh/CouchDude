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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using CouchDude.Configuration;
using CouchDude.Utils;

namespace CouchDude.Impl
{
	/// <summary>Session implementation.</summary>
	public partial class CouchSession : ISession
	{
		private const int UnitOfWorkFlashInProgressTimeout = 30000;
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();

		private readonly Settings settings;
		private readonly ICouchApi couchApi;
		private readonly IDatabaseApi databaseApi;
		private readonly SessionUnitOfWork unitOfWork;
		private readonly ManualResetEventSlim unitOfWorkFlashInProgressEvent = new ManualResetEventSlim(true);

		/// <constructor />
		public CouchSession(Settings settings, ICouchApi couchApi) 
			: this(settings != null? settings.DefaultDatabaseName: null, settings, couchApi) { }

		/// <constructor />
		public CouchSession(string databaseName, Settings settings, ICouchApi couchApi)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (settings.Incomplete) throw new ArgumentException("Settings are incomplete.", "settings");
			if (databaseName.HasNoValue()) throw new ArgumentNullException("databaseName");
			if (couchApi == null) throw new ArgumentNullException("couchApi");


			this.settings = settings;
			this.couchApi = couchApi;
			databaseApi = couchApi.Db(databaseName);
			unitOfWork = new SessionUnitOfWork(settings);
			Synchronously = new SynchronousSessionMethods(this);
		}

		/// <inheritdoc/>
		public ISynchronousSessionMethods Synchronously { get; private set; }

		/// <inheritdoc/>
		public ICouchApi RawApi { get { return couchApi; } }

		/// <inheritdoc/>
		public void Save<TEntity>(params TEntity[] entities) where TEntity : class
		{
			if (entities == null) throw new ArgumentNullException("entities");
			foreach (var entity in entities)
				Save(entity);
		}

		/// <inheritdoc/>
		public void Save<TEntity>(TEntity entity) where TEntity : class
		{
			if(ReferenceEquals(entity, null)) throw new ArgumentNullException("entity");

			WaitForFlushIfInProgress();
			
			var entityConfig = settings.GetConfig(typeof(TEntity));
			if (entityConfig == null)
				throw new EntityTypeNotRegistredException(typeof(TEntity));
			GenerateIdIfNeeded(entity, entityConfig, settings.IdGenerator);

			lock(unitOfWork)
				unitOfWork.AddNew(entity);
		}

		private void WaitForFlushIfInProgress()
		{
			Log.Info("Wating on session flush event");
			if(!unitOfWorkFlashInProgressEvent.Wait(UnitOfWorkFlashInProgressTimeout))
				throw new InvalidOperationException(
					string.Format(
						"Operation was aborted due session.SaveChanges() operation takes over {0} milliseconds to complete",
						UnitOfWorkFlashInProgressTimeout));
			Log.Info("Session flush event set - proceeding");
		}

		/// <inheritdoc/>
		public void Delete<TEntity>(params TEntity[] entities) where TEntity : class
		{
			if (entities == null) throw new ArgumentNullException("entities");
			foreach (var entity in entities)
				Delete(entity);
		}

		/// <inheritdoc/>
		public void Delete<TEntity>(TEntity entity) where TEntity : class
		{
			if(ReferenceEquals(entity, null)) throw new ArgumentNullException("entity");
			
			WaitForFlushIfInProgress();

			lock (unitOfWork)
				unitOfWork.MarkAsRemoved(entity);
		}

		/// <inheritdoc/>
		public Task<TEntity> Load<TEntity>(string entityId) where TEntity : class
		{
			if (string.IsNullOrWhiteSpace(entityId))
				throw new ArgumentNullException("entityId");

			WaitForFlushIfInProgress();
			
			// Attempt to use cache
			lock (unitOfWork)
			{
				object cachedEntity;
				if (unitOfWork.TryGetByEntityIdAndType(entityId, typeof(TEntity), out cachedEntity))
				{
					if (cachedEntity != null && !(cachedEntity is TEntity))
						throw new EntityTypeMismatchException(cachedEntity.GetType(), typeof(TEntity));
					return Task.Factory.StartNew(() => (TEntity)cachedEntity);
				}
			}
			
			var entityConfig = settings.GetConfig(typeof (TEntity));
			if (entityConfig == null)
				throw new EntityTypeNotRegistredException(typeof(TEntity));
			var docId = entityConfig.ConvertEntityIdToDocumentId(entityId);
			return databaseApi.RequestDocumentById(docId).ContinueWith(rt => {
				var document = rt.Result;
				if (document == null)
					return default(TEntity);
				lock (unitOfWork)
				{
					unitOfWork.UpdateWithDocument(document);
					object freshlyUpdatedEntity;
					unitOfWork.TryGetByEntityIdAndType(entityId, typeof (TEntity), out freshlyUpdatedEntity);

					if(freshlyUpdatedEntity != null && !(freshlyUpdatedEntity is TEntity))
						throw new EntityTypeMismatchException(document.Type, typeof(TEntity));

					return (TEntity)freshlyUpdatedEntity;
				}
			});
		}

		/// <inheritdoc/>
		public Task StartSavingChanges()
		{
			return Task.Factory
				.StartNew(
					() => {
						WaitForFlushIfInProgress();
						Log.Info("Reseting session flush event");
						unitOfWorkFlashInProgressEvent.Reset();

						databaseApi
							.BulkUpdate(bulk => unitOfWork.ApplyChanges(bulk))
							.ContinueWith(HandleChangesSaveCompleted, TaskContinuationOptions.AttachedToParent);
					});
		}

		private void HandleChangesSaveCompleted(Task<IDictionary<string, DocumentInfo>> saveChangesTask)
		{
			// releasing mutex before locking on unitOfWork to prevent dead lock
			Log.Info("Setting session flush event due save operation completion");
			unitOfWorkFlashInProgressEvent.Set();

			if (saveChangesTask.IsFaulted)
			{
				var aggregateException = saveChangesTask.Exception;
				if (aggregateException != null)
					throw aggregateException.Flatten();
			}
			lock (unitOfWork)
				unitOfWork.UpdateRevisions(saveChangesTask.Result.Values);
		}

		private static void GenerateIdIfNeeded(
			object entity, IEntityConfig entityConfiguration, IIdGenerator idGenerator)
		{
			var id = entityConfiguration.GetId(entity);
			if (id == null)
			{
				var generatedId = idGenerator.GenerateId();
				Debug.Assert(!string.IsNullOrEmpty(generatedId));
				entityConfiguration.SetId(entity, generatedId);
			}
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
				unitOfWork.Clear();
		}
	}
}