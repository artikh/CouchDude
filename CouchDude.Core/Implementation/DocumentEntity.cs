using System;
using System.Diagnostics.Contracts;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CouchDude.Core.Conventions;

namespace CouchDude.Core.Implementation
{
	/// <summary>Represents CouchDB document - entity relationship.</summary>
	internal class DocumentEntity
	{
		private const string IdPropertyName = "_id";
		private const string RevisionPropertyName = "_rev";
		private const string TypePropertyName = "type";

		private string revision;

		private readonly SpecialPropertyDescriptor revisionPropertyDescriptor;

		// ReSharper disable NotAccessedField.Local
		private readonly SpecialPropertyDescriptor idPropertyDescriptor;
		// ReSharper restore NotAccessedField.Local

		/// <summary>Entity identitifier.</summary>
		public readonly string EntityId;

		/// <summary>Document identitifier.</summary>
		public readonly string DocumentId;

		/// <summary>Currently loaded revision of the document/entity.</summary>
		public string Revision
		{
			get { return revision; }
			set
			{
				if (revision == value) return;

				revisionPropertyDescriptor.SetIfAble(Entity, value);
				if (Document != null)
					SetRevisionPropertyOnDocument(value, Document);
				revision = value;
			}
		}

		/// <summary>Type of the entity bound to the document.</summary>
		public Type EntityType { get; private set; }

		/// <summary>Document property type string.</summary>
		public string DocumentType { get; private set; }

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
			var idPropertyDescriptor = settings.GetIdPropertyDescriptor<TEntity>();
			var id = GetIdOrGenerateOne(entity, idPropertyDescriptor, settings);

			var revisionPropertyDescriptor = settings.GetRevPropertyDescriptor<TEntity>();
			var revision = revisionPropertyDescriptor.GetIfAble(entity);

			var documentType = settings.GetDocumentType<TEntity>();
			if (documentType == null)
				throw new ConfigurationException("Type {0} have not been registred.", typeof(TEntity));

			return new DocumentEntity(
				idPropertyDescriptor, revisionPropertyDescriptor, 
				id, revision, typeof(TEntity), documentType, entity);
		}

		private static string GetIdOrGenerateOne<TEntity>(
			TEntity entity, SpecialPropertyDescriptor idPropertyDescriptor, Settings settings) 
		{
			var id = idPropertyDescriptor.GetIfAble(entity);
			if(id == null)
			{
				var generatedId = settings.IdGenerator.GenerateId();
				Contract.Assert(!string.IsNullOrEmpty(generatedId));
				idPropertyDescriptor.SetIfAble(entity, generatedId);
				id = idPropertyDescriptor.GetIfAble(entity);
			}
			if (id == null)
				throw new ArgumentException(
					"Entity's ID property should be set or settable.", "entity");
			return id;
		}

		/// <summary>Creates instance from JSON document reading it form 
		/// provided text reader.</summary>
		public static DocumentEntity FromJson<TEntity>(JObject document, Settings settings, bool throwOnTypeMismatch = true) 
			where TEntity : class
		{
			var docId = document.GetRequiredProperty(IdPropertyName);
			var revision = document.GetRequiredProperty(RevisionPropertyName);

			var documentType = GetDocumnetType(document, throwOnTypeMismatch);
			if (documentType == null)
				return null;

			var expectedType = settings.GetDocumentType<TEntity>();
			if (expectedType == null)
				throw new ConfigurationException("Type {0} have not been registred.", typeof(TEntity));

			if (expectedType != documentType)
				if (throwOnTypeMismatch)
					throw new EntityTypeMismatchException(documentType, typeof(TEntity));
				else
					return null;

			if (!docId.StartsWith(documentType + "."))
				if (throwOnTypeMismatch)
					throw new CouchResponseParseException("Document IDs should be prefixed by their type.");
				else
					return null;

			var entityId = docId.Substring(documentType.Length + 1);

			TEntity entity;
			using (var reader = new JTokenReader(document))
				entity = JsonSerializer.Instance.Deserialize<TEntity>(reader);

			var idPropertyDescriptor = settings.GetIdPropertyDescriptor<TEntity>();
			idPropertyDescriptor.SetIfAble(entity, entityId);
			var revisionPropertyDescriptor = settings.GetRevPropertyDescriptor<TEntity>();
			revisionPropertyDescriptor.SetIfAble(entity, revision);

			return new DocumentEntity(
				idPropertyDescriptor, revisionPropertyDescriptor, entityId, revision, typeof(TEntity), documentType, entity, document);
		}

		private static string GetDocumnetType(JObject document, bool throwOnTypeMismatch)
		{
			var propertyValue = document[TypePropertyName] as JValue;
			if (propertyValue != null)
			{
				var value = propertyValue.Value<string>();
				if (!string.IsNullOrWhiteSpace(value))
					return value;
			}

			if (!throwOnTypeMismatch) 
				return null;

			throw new CouchResponseParseException(
				"Required field '{0}' have not found on document. " 
					+ "Type is required by CouchDude itself so it colud do it magic stuff:\n {1}",
				TypePropertyName,
				"Type is required by CouchDude itself so it colud do it magic stuff",
				document.ToString(Formatting.None)
				);
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

		private static string GetDocumentId(string entityId, string documentType)
		{
			return documentType + "." + entityId;
		}

		private DocumentEntity(
			SpecialPropertyDescriptor idPropertyDescriptor,
			SpecialPropertyDescriptor revisionPropertyDescriptor,  
			string entityId, 
			string revision, 
			Type entityType, 
			string documentType, 
			object entity, 
			JObject document = null)
		{
			if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("entityId");
			if (string.IsNullOrEmpty(documentType)) throw new ArgumentNullException("documentType");
			if (entityType == null) throw new ArgumentNullException("entityType");
			if (entity == null) throw new ArgumentNullException("entity");
			Contract.EndContractBlock();

			EntityId = entityId;
			DocumentId = GetDocumentId(entityId, documentType);
			this.revision = revision;
			EntityType = entityType;
			DocumentType = documentType;
			Entity = entity;
			Document = document;
			this.idPropertyDescriptor = idPropertyDescriptor;
			this.revisionPropertyDescriptor = revisionPropertyDescriptor;
		}

		private JObject SerializeToDocument()
		{
			JObject document;
			using (var writer = new JTokenWriter())
			{
				JsonSerializer.Instance.Serialize(writer, Entity);
				writer.Flush();
				document = (JObject)writer.Token;
			}

			document.AddFirst(new JProperty(TypePropertyName, DocumentType));
			if (Revision != null)
				document.AddFirst(new JProperty(RevisionPropertyName, Revision));
			document.AddFirst(new JProperty(IdPropertyName, DocumentId));
			return document;
		}

		private static void SetRevisionPropertyOnDocument(string revision, JObject document) 
		{
			var newRevisionValue = JToken.FromObject(revision);
			var revisionProperty = document.Property(RevisionPropertyName);
			if (revisionProperty != null)
				revisionProperty.Value = newRevisionValue;
			else
			{
				revisionProperty = new JProperty(RevisionPropertyName, newRevisionValue);
				var idProperty = document.Property(IdPropertyName);
				if (idProperty != null)
					idProperty.AddAfterSelf(revisionProperty);
				else
					document.Add(revisionProperty);
			}
		}
	}
}
