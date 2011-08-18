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

namespace CouchDude.Impl
{
	/// <summary>Session Unit of Work cache.</summary>
	/// <remarks>Instance methods are not guaranteed to be thread-safe.</remarks>
	internal class SessionUnitOfWork
	{
		private readonly IDictionary<object, DocumentEntity> instanceSet = new Dictionary<object, DocumentEntity>();
		private readonly IDictionary<string, DocumentEntity> idMap = new Dictionary<string, DocumentEntity>();

		private DocumentEntity this[string entityId, Type entityType]
		{
			get
			{
				var entityKey = GetEntityKey(entityId, entityType);
				DocumentEntity documentEntity;
				idMap.TryGetValue(entityKey, out documentEntity);
				return documentEntity;
			}
			set { idMap[GetEntityKey(entityId, entityType)] = value; }
		}

		private DocumentEntity this[object instance]
		{
			get
			{
				DocumentEntity documentEntity;
				instanceSet.TryGetValue(instance, out documentEntity);
				return documentEntity;
			}
			set { instanceSet[instance] = value; }
		}

		private static string GetEntityKey(string entityId, Type entityType)
		{
			return string.Concat(entityType.FullName, "::", entityId);
		}

		/// <summary>Tries to get document entity form the cache via ID.</summary>
		public DocumentEntity TryGet(string entityId, Type entityType)
		{
			return this[entityId, entityType];
		}

		/// <summary>Tries to get document entity form the cache via entity reverence.</summary>
		public DocumentEntity TryGet(object entity)
		{
			return this[entity];
		}

		/// <summary>Places provided document entity to the cache or if there is 
		/// entity of same ID in cache already replaces it with one from cache.</summary>
		public DocumentEntity PutOrReplace(DocumentEntity documentEntity)
		{
			if (documentEntity == null) throw new ArgumentNullException("documentEntity");

			var cachedDocumentEntity = this[documentEntity.EntityId, documentEntity.EntityType];
			return cachedDocumentEntity ?? Put(documentEntity);
		}

		/// <summary>Places provided document entity to the cache.</summary>
		public DocumentEntity Put(DocumentEntity documentEntity)
		{
			this[documentEntity.Entity] = documentEntity;
			this[documentEntity.EntityId, documentEntity.EntityType] = documentEntity;
			return documentEntity;
		}

		public void Remove(DocumentEntity documentEntity)
		{
			instanceSet.Remove(documentEntity.Entity);
			if(documentEntity.DocumentId != null)
				idMap.Remove(documentEntity.DocumentId);
		}

		/// <summary>Determines if paticular instance is in the cache.</summary>
		public bool Contains(DocumentEntity documentEntity)
		{
			return idMap.ContainsKey(documentEntity.DocumentId) 
			       || instanceSet.ContainsKey(documentEntity.Entity);
		}

		/// <summary>Marks <see cref="DocumentEntity"/> as deleted form the unit of work.</summary>
		public void MarkAsDeleted(DocumentEntity documentEntity)
		{
			
		}

		/// <summary>Returs all cached documents.</summary>
		public IEnumerable<DocumentEntity> DocumentEntities { get { return idMap.Values; } }

		/// <summary>Clears the cache.</summary>
		public void Clear()
		{
			instanceSet.Clear();
			idMap.Clear();
		}
	}
}
