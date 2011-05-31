using System;
using System.Collections.Generic;

namespace CouchDude.Core.Impl
{
	/// <summary>Document and entity cache.</summary>
	/// <remarks>Instance methods are not guaranteed to be thread-safe.</remarks>
	internal class DocumentEntityCache
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
			idMap.Remove(documentEntity.DocumentId);
		}

		/// <summary>Determines if paticular instance is in the cache.</summary>
		public bool Contains(DocumentEntity documentEntity)
		{
			return idMap.ContainsKey(documentEntity.DocumentId) 
			       || instanceSet.ContainsKey(documentEntity.Entity);
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
