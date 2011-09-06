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
using CouchDude.Utils;

namespace CouchDude.Impl
{
	/// <summary>Unit of work for CRUD implementation for <see cref="ISession"/>.</summary>
	/// <remarks>Instance methods are not thread safe. Instance should be protected before 
	/// accessing in parallel.</remarks>
	class SessionUnitOfWork
	{
		private readonly IDictionary<object, DocumentEntity> entityMap = new Dictionary<object, DocumentEntity>();
		private readonly IDictionary<Tuple<string, Type>, DocumentEntity> entityIdAndTypeMap = 
			new Dictionary<Tuple<string, Type>, DocumentEntity>();
		private readonly IDictionary<string, DocumentEntity> documentIdMap = new Dictionary<string, DocumentEntity>();

		private readonly IEntityConfigRepository entityConfigRepository;

		/// <constructor />
		public SessionUnitOfWork(IEntityConfigRepository entityConfigRepository)
		{
			this.entityConfigRepository = entityConfigRepository;
		}


		/// <summary>Attaches already persisted document entity to the unit.</summary>
		public void Attach(object entity, bool markAsUnchanged = false)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			var documentEntity = GetDocumentEntity(entity);
			if (!documentEntity.HavePersisted) throw new ArgumentException("Persisted document entity expected", "entity");

			if (markAsUnchanged)
				documentEntity.MapIfChanged();
			RegisterDocumentEntity(documentEntity);
		}

		/// <summary>Adds new document entity (without document part actually) to the unit.</summary>
		public void AddNew(object entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			var documentEntity = GetDocumentEntity(entity);
			if (documentEntity.HavePersisted) throw new ArgumentException("Transient document entity expected", "entity");
			RegisterDocumentEntity(documentEntity);
		}

		/// <summary>Marks document entity as deleted from the store forsing other <see cref="SessionUnitOfWork"/> methods
		/// to behave as if it have been already removed.</summary>
		public void MarkAsRemoved(object entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			var documentEntity = GetDocumentEntity(entity);
			documentEntity.HaveRemoved = true;
			RegisterDocumentEntity(documentEntity);
		}

		/// <summary>Translates session unit of work to CouchApi bulk update unit of work.</summary>
		public bool ApplyChanges(IBulkUpdateBatch work)
		{
			if (work == null) throw new ArgumentNullException("work");

			bool haveChanged = false;
			foreach (var documentEntity in entityMap.Values)
			{
				if (documentEntity.HaveRemoved)
				{
					if (documentEntity.HavePersisted)
					{
						work.Delete(documentEntity.DocumentId, documentEntity.Revision);
						haveChanged = true;
					}
				}
				else
				{
					var changed = documentEntity.MapIfChanged();
					if (!documentEntity.HavePersisted)
					{
						work.Create(documentEntity.Document);
						haveChanged = true;
					}
					else if (changed)
					{
						work.Update(documentEntity.Document);
						haveChanged = true;
					}
				}
			}
			return haveChanged;
		}

		public bool TryGetByEntityIdAndType(string entityId, Type type, out object cachedEntity)
		{
			DocumentEntity documentEntity;
			cachedEntity = null;
			if(entityIdAndTypeMap.TryGetValue(new Tuple<string, Type>(entityId, type), out documentEntity))
			{
				if (!documentEntity.HaveRemoved) 
					cachedEntity = documentEntity.Entity;
				return true;
			}
			return false;
		}

		/// <summary>Attempts to retrive cached entity by it's documentID (withch shoud be unique)</summary>
		public bool TryGetByDocumentId(string documentId, out object cachedEntity)
		{
			DocumentEntity documentEntity;
			cachedEntity = null;
			if (documentIdMap.TryGetValue(documentId, out documentEntity) && !documentEntity.HaveRemoved)
			{
				if (!documentEntity.HaveRemoved)
					cachedEntity = documentEntity.Entity;
				return true;
			}
			
			return false;
		}

		/// <summary>Updates cache with provided document.</summary>
		public void UpdateWithDocument(IDocument document)
		{
			if(document == null || document.Id == null) return;

			DocumentEntity documentEntity;
			if (!documentIdMap.TryGetValue(document.Id, out documentEntity) && document.Revision.HasValue())
			{
				documentEntity = DocumentEntity.TryFromDocument(document, entityConfigRepository);
				if (documentEntity != null)
					RegisterDocumentEntity(documentEntity);
			}
		}

		public void Clear()
		{
			entityIdAndTypeMap.Clear();
			entityMap.Clear();
		}

		/// <summary>Maps entity to document entity using unit of work cache.</summary>
		internal DocumentEntity GetDocumentEntity(object entity)
		{
			DocumentEntity documentEntity;
			if (!entityMap.TryGetValue(entity, out documentEntity))
				documentEntity = RegisterDocumentEntity(
					DocumentEntity.FromEntity(entity, entityConfigRepository));
			return documentEntity;
		}

		public void UpdateRevisions(IEnumerable<DocumentInfo> updatedDocumentInfo) 
		{
			foreach (var documentInfo in updatedDocumentInfo)
			{
				DocumentEntity documentEntity;
				if (documentIdMap.TryGetValue(documentInfo.Id, out documentEntity))
				{
					if (documentEntity.HaveRemoved)
						RemoveDocumentEntity(documentEntity);

					documentEntity.Revision = documentInfo.Revision;
				}
			}
		}

		private DocumentEntity RegisterDocumentEntity(DocumentEntity documentEntity)
		{
			entityMap[documentEntity.Entity] = documentEntity;

			foreach (var identity in GetDocumentEntityIdentities(documentEntity))
				entityIdAndTypeMap[identity] = documentEntity;

			documentIdMap[documentEntity.DocumentId] = documentEntity;

			return documentEntity;
		}

		private void RemoveDocumentEntity(DocumentEntity documentEntity)
		{
			entityMap.Remove(documentEntity.Entity);
			
			foreach (var identity in GetDocumentEntityIdentities(documentEntity))
				entityIdAndTypeMap.Remove(identity);

			documentIdMap.Remove(documentEntity.DocumentId);
		}

		private IEnumerable<Tuple<string, Type>> GetDocumentEntityIdentities(DocumentEntity documentEntity)
		{
			return entityConfigRepository
				.GetAllRegistredBaseTypes(documentEntity.EntityType)
				.Select(type => new Tuple<string, Type>(documentEntity.EntityId, type));
		}
	}
}