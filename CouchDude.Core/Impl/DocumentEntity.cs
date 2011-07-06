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
		public string DocumentId { get { return Document == null ? null : Document.GetRequiredProperty(EntitySerializer.IdPropertyName); } }

		/// <summary>Currently loaded revision of the document/entity.</summary>
		public string Revision
		{
			get { return entityConfiguration.GetRevision(Entity); }
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

		/// <summary>Database document raw.</summary>
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
			var entityConfiguration = settings.GetConfig(entity);
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
				entityConfiguration.TrySetId(entity, generatedId);
			}
		}

		/// <summary>Creates instance from JSON document reading it form 
		/// provided text reader. If any error does occur returns <c>null</c>.</summary>
		public static DocumentEntity TryFromJson<TEntity>(JObject document, Settings settings)
			where TEntity : class
		{
			var documentType = GetDocumnetType(document);
			if (!string.IsNullOrWhiteSpace(documentType))
			{
				var entityConfiguration = settings.GetConfigFromDocumentType(documentType);
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

			var entityConfiguration = settings.GetConfigFromDocumentType(documentType);
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
			return EntitySerializer.Serialize(Entity, entityConfiguration);
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
