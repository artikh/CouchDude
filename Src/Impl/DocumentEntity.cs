﻿#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
using CouchDude.Configuration;
using CouchDude.Utils;

namespace CouchDude.Impl
{
	/// <summary>Represents CouchDB document - entity relationship.</summary>
	internal class DocumentEntity
	{
		/// <summary>Document entity configuration.</summary>
		private readonly IEntityConfig entityConfiguration;

		private readonly ISerializer serializer;

		private string entityId;
		/// <summary>Entity identitifier.</summary>
		public string EntityId { get { return entityId ?? (entityId = entityConfiguration.GetId(Entity)); } }
		
		/// <summary>Document identitifier.</summary>
		public string DocumentId
		{
			get { return entityConfiguration.ConvertEntityIdToDocumentId(EntityId); }
		}

		/// <summary>Currently loaded revision of the document/entity.</summary>
		/// <remarks>First source of truth is always entity.</remarks>
		public string Revision
		{
			get
			{
				if (Entity != null && entityConfiguration.IsRevisionPresent)
				{
					var entityRevision = entityConfiguration.GetRevision(Entity);
					if (entityRevision != null)
						return entityRevision;
				}

				return Document != null ? Document.Revision : null;
			}
			set
			{
				if (Revision == value) return;

				entityConfiguration.SetRevision(Entity, value);
				if (Document != null)
					Document.Revision = value;
			}
		}

		/// <summary>Type of the entity bound to the document.</summary>
		public Type EntityType { get { return entityConfiguration.EntityType; } }

		/// <summary>Document property type string.</summary>
		public string DocumentType { get { return entityConfiguration.DocumentType; } }

		/// <summary>Entity instance.</summary>
		public object Entity { get; private set; }

		/// <summary>Idicates if entity have been persisted.</summary>
		public bool HavePersisted { get { return Revision.HasValue(); } }

		/// <summary>Idicates if entity have been deleted.</summary>
		public bool HaveRemoved { get; set; }

		/// <summary>Return entity casted to specified type.</summary>
		public TEntity GetEntity<TEntity>() where TEntity: class
		{
			return (TEntity) Entity;
		}

		/// <summary>Database raw document.</summary>
		public Document Document { get; private set; }

		/// <summary>Creates instance from entity.</summary>
		public static DocumentEntity FromEntity(object entity, Settings settings)
		{
			var entityType = entity.GetType();
			var entityConfiguration = settings.GetConfig(entityType);
			if (entityConfiguration == null)
				throw new ConfigurationException("Entity type {0} have not been registred.", entityType);
			
			return new DocumentEntity(entityConfiguration, settings.Serializer, entity);
		}

		/// <summary>Creates entity/document pair from CouchDB document. If any error does occur returns <c>null</c>.</summary>
		public static DocumentEntity TryFromDocument(Document document, Settings settings)
		{
			var serializer = settings.Serializer;
			if (document != null && !string.IsNullOrWhiteSpace(document.Type))
			{
				var entityConfiguration = settings.GetConfig(document.Type);
				if (entityConfiguration != null)
				{
					var entity = serializer.ConvertFromJson(entityConfiguration, document.RawJsonObject, throwOnError: false);
					if (entity != null)
						return new DocumentEntity(entityConfiguration, serializer, entity, document);
				}
			}

			return null;
		}

		/// <summary>Creates entity/document pair from CouchDB document.</summary>
		public static DocumentEntity FromDocument(Document document, Settings settings) 
		{
			if(string.IsNullOrWhiteSpace(document.Type))
				throw new DocumentTypeMissingException(document.ToString());

			var entityConfiguration = settings.GetConfig(document.Type);
			if (entityConfiguration == null)
				throw new DocumentTypeNotRegistredException(document.Type);

			var serializer = settings.Serializer;
			var entity = serializer.ConvertFromJson(entityConfiguration, document.RawJsonObject, throwOnError: true);
			return new DocumentEntity(entityConfiguration, serializer, entity, document); 
		}

		/// <summary>Maps entity to the JSON document.</summary>
		public void DoMap()
		{
			Document = SerializeToDocument();
		}

		/// <summary>Activly checks if entity is differ then JSON document.</summary>
		public bool MapIfChanged()
		{
			var updatedDocument = SerializeToDocument();
			var changed = Document == null || !Document.Equals(updatedDocument);
			if (changed)
				Document = updatedDocument;
			return changed;
		}
		
		private DocumentEntity(
			IEntityConfig entityConfiguration,
			ISerializer serializer,
			object entity, 
			Document document = null)
		{
			if (entityConfiguration == null) throw new ArgumentNullException("entityConfiguration");
			if (entity == null) throw new ArgumentNullException("entity");
			
			Entity = entity;
			Document = document;
			this.entityConfiguration = entityConfiguration;
			this.serializer = serializer;
		}

		private Document SerializeToDocument()
		{
			var newVersionOfDocument = 
				new Document(serializer.ConvertToJson(Entity, entityConfiguration, throwOnError: true));

			if (Document != null)
			{
				// if revision info is not stored in entity it should be copied from current document (if present)
				if (!entityConfiguration.IsRevisionPresent)
					newVersionOfDocument.Revision = Document.Revision;
			}
			return newVersionOfDocument;
		}
	}
}
