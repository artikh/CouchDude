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
using System.Diagnostics.Contracts;
using System.IO;
using CouchDude.Core.Configuration;
using CouchDude.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Impl
{
	/// <summary>Represents CouchDB document - entity relationship.</summary>
	internal class DocumentEntity
	{
		/// <summary>Document entity configuration.</summary>
		private readonly IEntityConfig entityConfiguration;

		private string entityId;
		/// <summary>Entity identitifier.</summary>
		public string EntityId { get { return entityId ?? (entityId = entityConfiguration.GetId(Entity)); } }
		
		/// <summary>Document identitifier.</summary>
		public string DocumentId
		{
			get
			{
				return entityConfiguration.ConvertEntityIdToDocumentId(EntityId);
			}
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

				return Document != null ? Document.Value<string>(EntitySerializer.RevisionPropertyName) : null;
			}
			set
			{
				if (Revision == value) return;

				entityConfiguration.SetRevision(Entity, value);
				if (Document != null)
					SetRevisionPropertyOnDocument(value, Document);
			}
		}

		/// <summary>Type of the entity bound to the document.</summary>
		public Type EntityType { get { return entityConfiguration.EntityType; } }

		/// <summary>Document property type string.</summary>
		public string DocumentType { get { return entityConfiguration.DocumentType; } }

		/// <summary>Entity instance.</summary>
		public object Entity { get; private set; }

		/// <summary>Return entity casted to specified type.</summary>
		public TEntity GetEntity<TEntity>() where TEntity: class
		{
			return (TEntity) Entity;
		}

		/// <summary>Database raw document.</summary>
		public JObject Document { get; private set; }

		/// <summary>Writes document to provided writer.</summary>
		public void WriteTo(TextWriter writer)
		{
			var document = SerializeToDocument();
			using (var jsonWriter = new JsonTextWriter(writer))
				document.WriteTo(jsonWriter);
		}

		/// <summary>Creates instance from entity.</summary>
		public static DocumentEntity FromEntity<TEntity>(TEntity entity, Settings settings)
			where TEntity: class
		{
			var entityConfiguration = settings.GetConfig(typeof(TEntity));
			if (entityConfiguration == null)
				throw new ConfigurationException("Entity type {0} have not been registred.", typeof(TEntity));
			GenerateIdIfNeeded(entity, entityConfiguration, settings.IdGenerator);

			return new DocumentEntity(entityConfiguration, entity);
		}

		private static void GenerateIdIfNeeded(
			object entity, IEntityConfig entityConfiguration, IIdGenerator idGenerator) 
		{
			var id = entityConfiguration.GetId(entity);
			if(id == null)
			{
				var generatedId = idGenerator.GenerateId();
				Contract.Assert(!string.IsNullOrEmpty(generatedId));
				entityConfiguration.SetId(entity, generatedId);
			}
		}

		/// <summary>Creates instance from JSON document reading it form 
		/// provided text reader. If any error does occur returns <c>null</c>.</summary>
		public static DocumentEntity TryFromJson<TEntity>(JObject document, Settings settings)
		{
			var documentType = GetDocumnetType(document);
			if (!string.IsNullOrWhiteSpace(documentType))
			{
				var entityConfiguration = settings.GetConfig(documentType);
				if (entityConfiguration != null && entityConfiguration.IsCompatibleWith<TEntity>())
				{
					var entity = EntitySerializer.TryDeserialize(document, entityConfiguration);
					if (entity != null)
						return new DocumentEntity(entityConfiguration, entity, document);
				}
			}

			return null;
		}

		/// <summary>Creates instance from JSON document reading it form 
		/// provided text reader.</summary>
		public static DocumentEntity FromJson<TEntity>(JObject document, Settings settings) 
			where TEntity : class
		{
			var documentType = GetDocumnetType(document);
			if(string.IsNullOrWhiteSpace(documentType))
				throw new DocumentTypeMissingException(document);

			var entityConfiguration = settings.GetConfig(documentType);
			if (entityConfiguration == null)
				throw new DocumentTypeNotRegistredException(documentType);

			if (!entityConfiguration.IsCompatibleWith<TEntity>())
				throw new EntityTypeMismatchException(documentType, typeof(TEntity));

			var entity = EntitySerializer.Deserialize(document, entityConfiguration);
			return new DocumentEntity(entityConfiguration, entity, document);}

		private static string GetDocumnetType(JObject document)
		{
			var propertyValue = document[EntitySerializer.TypePropertyName] as JValue;
			if (propertyValue != null)
			{
				var value = propertyValue.Value<string>();
				if (!string.IsNullOrWhiteSpace(value))
					return value;
			}
			return null;
		}

		/// <summary>Maps entity to the JSON document.</summary>
		public void DoMap()
		{
			Document = SerializeToDocument();
		}

		/// <summary>Activly checks if entity is differ then JSON document.</summary>
		public bool CheckIfChanged()
		{
			return Document != null && !new JTokenEqualityComparer().Equals(Document, SerializeToDocument());
		}
		
		private DocumentEntity(
			IEntityConfig entityConfiguration,
			object entity, 
			JObject document = null)
		{
			if (entityConfiguration == null) throw new ArgumentNullException("entityConfiguration");
			if (entity == null) throw new ArgumentNullException("entity");
			Contract.EndContractBlock();

			Entity = entity;
			Document = document;
			this.entityConfiguration = entityConfiguration;
		}

		private JObject SerializeToDocument()
		{
			return EntitySerializer.Serialize(Entity, entityConfiguration, Revision);
		}

		private static void SetRevisionPropertyOnDocument(string revision, JObject document) 
		{
			var newRevisionValue = JToken.FromObject(revision);
			var revisionProperty = document.Property(EntitySerializer.RevisionPropertyName);
			if (revisionProperty != null)
				revisionProperty.Value = newRevisionValue;
			else
			{
				revisionProperty = new JProperty(EntitySerializer.RevisionPropertyName, newRevisionValue);
				var idProperty = document.Property(EntitySerializer.IdPropertyName);
				if (idProperty != null)
					idProperty.AddAfterSelf(revisionProperty);
				else
					document.Add(revisionProperty);
			}
		}
	}
}
