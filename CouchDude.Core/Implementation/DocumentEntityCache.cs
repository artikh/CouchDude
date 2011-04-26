using System;
using System.Collections.Generic;

namespace CouchDude.Core.Implementation
{
	/// <summary>Document and entity cache.</summary>
	internal class DocumentEntityCache
	{
		private readonly IDictionary<object, DocumentEntity> instanceSet = new Dictionary<object, DocumentEntity>();
		readonly IDictionary<string, DocumentEntity> idMap = new Dictionary<string, DocumentEntity>();

		/// <summary>Determines if paticular instance is in the cache.</summary>
		public bool Contains(DocumentEntity documentEntity)
		{
			return idMap.ContainsKey(documentEntity.EntityId) 
				|| instanceSet.ContainsKey(documentEntity.Entity);
		}
		
		/// <summary>Tries to get document entity form the cache via ID.</summary>
		public DocumentEntity TryGet(string id)
		{
			DocumentEntity documentEntity;
			idMap.TryGetValue(id, out documentEntity);
			return documentEntity;
		}
		
		/// <summary>Tries to get document entity form the cache via entity reverence.</summary>
		public DocumentEntity TryGet(object entity)
		{
			DocumentEntity documentEntity;
			instanceSet.TryGetValue(entity, out documentEntity);
			return documentEntity;
		}

		/// <summary>Places provided document entity to the cache or if there is 
		/// entity of same ID in cache already replaces it with one from cache.</summary>
		public void PutOrReplace(ref DocumentEntity documentEntity)
		{
			if (documentEntity == null) throw new ArgumentNullException("documentEntity");

			// Replacing entity in outer scope with one from cache if there is one
			if (!idMap.TryGetValue(documentEntity.EntityId, out documentEntity))
			{
				// if not - putting one from outer scope in cache
				Put(documentEntity);
			}
		}

		/// <summary>Places provided document entity to the cache.</summary>
		public void Put(DocumentEntity documentEntity)
		{
			instanceSet.Add(documentEntity.Entity, documentEntity);
			idMap[documentEntity.EntityId] = documentEntity;
		}

		public void Remove(DocumentEntity documentEntity)
		{
			instanceSet.Remove(documentEntity.Entity);
			idMap.Remove(documentEntity.EntityId);
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
