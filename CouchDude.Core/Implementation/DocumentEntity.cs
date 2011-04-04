using System;
using System.Diagnostics.Contracts;
using System.IO;
using CouchDude.Core.Conventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Core.Implementation
{
	/// <summary>Represents CouchDB document - entity relationship.</summary>
	internal class DocumentEntity
	{
		private const string IdPropertyName = "_id";
		private const string RevisionPropertyName = "_rev";
		private const string TypePropertyName = "type";

		[ThreadStatic]
		private static JsonSerializer serializer;

		private string revision;

		private readonly SpecialPropertyDescriptor revisionPropertyDescriptor;

		private readonly SpecialPropertyDescriptor idPropertyDescriptor;

		/// <summary>Document/entity identitifier.</summary>
		public readonly string Id;

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
			where TEntity: new()
		{
			var idPropertyDescriptor = settings.GetIdPropertyDescriptor<TEntity>();
			var id = idPropertyDescriptor.GetIfAble(entity);
			var revisionPropertyDescriptor = settings.GetRevPropertyDescriptor<TEntity>();
			var revision = revisionPropertyDescriptor.GetIfAble(entity);

			var documentType = settings.GetDocumentType<TEntity>();

			return new DocumentEntity(
				idPropertyDescriptor, revisionPropertyDescriptor, 
				id, revision, typeof(TEntity), documentType, entity);
		}

		/// <summary>Creates instance from JSON document reading it form 
		/// provided text reader.</summary>
		public static DocumentEntity FromJson<TEntity>(JObject document, Settings settings) 
			where TEntity : new()
		{
			var id = document.GetRequiredProperty(IdPropertyName);
			var revision = document.GetRequiredProperty(RevisionPropertyName);
			var type = document.GetRequiredProperty(
				TypePropertyName, 
				"Type is required by CouchDude itself so it colud do it magic stuff");

			var expectedType = settings.GetDocumentType<TEntity>();
			if (expectedType != type)
				throw new EntityTypeMismatchException(type, typeof (TEntity));

			TEntity entity;
			using (var reader = new JTokenReader(document))
				entity = Serializer.Deserialize<TEntity>(reader);

			var idPropertyDescriptor = settings.GetIdPropertyDescriptor<TEntity>();
			idPropertyDescriptor.SetIfAble(entity, id);
			var revisionPropertyDescriptor = settings.GetRevPropertyDescriptor<TEntity>();
			revisionPropertyDescriptor.SetIfAble(entity, revision);

			return new DocumentEntity(idPropertyDescriptor, revisionPropertyDescriptor, id, revision, typeof(TEntity), type, entity, document);
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
			SpecialPropertyDescriptor idPropertyDescriptor,
			SpecialPropertyDescriptor revisionPropertyDescriptor,  
			string id, string revision, 
			Type entityType, string documentType, object entity, JObject document = null)
		{
			if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
			if (string.IsNullOrEmpty(documentType)) throw new ArgumentNullException("documentType");
			if (entityType == null) throw new ArgumentNullException("entityType");
			if (entity == null) throw new ArgumentNullException("entity");
			Contract.EndContractBlock();

			Id = id;
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
				Serializer.Serialize(writer, Entity);
				writer.Flush();
				document = (JObject)writer.Token;
			}

			document.AddFirst(new JProperty(TypePropertyName, DocumentType));
			if (Revision != null)
				document.AddFirst(new JProperty(RevisionPropertyName, Revision));
			document.AddFirst(new JProperty(IdPropertyName, Id));
			return document;
		}

		private static JsonSerializer Serializer
		{
			get { return serializer ?? (serializer = CreateSerializer()); }
		}

		private static JsonSerializer CreateSerializer()
		{
			var settings = new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Converters = { new IsoDateTimeConverter() }
			};

			return JsonSerializer.Create(settings);
		}

		public void SetId(string id)
		{
			var newIdValue = JToken.FromObject(id);
			idPropertyDescriptor.SetIfAble(Entity, id);
			if(Document != null)
			{
				var idProperty = Document.Property(IdPropertyName);
				if (idProperty != null)
					idProperty.Value = newIdValue;
				else
					Document.AddFirst(new JProperty(IdPropertyName, newIdValue));
			}
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
