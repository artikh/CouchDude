using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using CouchDude.Core.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace CouchDude.Core.Impl
{
	internal static class EntitySerializer
	{
		public const string IdPropertyName = "_id";
		public const string RevisionPropertyName = "_rev";
		public const string TypePropertyName = "type";

		private static readonly ConcurrentDictionary<IEntityConfig, JsonSerializer> Serializers =
			new ConcurrentDictionary<IEntityConfig, JsonSerializer>();

		public static object Deserialize(JObject document, IEntityConfig entityConfig)
		{
			if (document == null)
				throw new ArgumentNullException("document");
			if (entityConfig == null)
				throw new ArgumentNullException("entityConfig");

			string documentId, revision;
			GetDocumentTypeAndRevision(document, out documentId, out revision);
			
			if(string.IsNullOrWhiteSpace(documentId))
				throw new DocumentIdMissingException(document);

			var entityId = entityConfig.ConvertDocumentIdToEntityId(documentId);
			if(string.IsNullOrWhiteSpace(entityId))
				throw new InvalidOperationException(
					"IEntityConfig.ConvertDocumentIdToEntityId() should not ever return null, empty or whitespace string.");

			var entity = DeserializeFromJObject(document, entityConfig);
			entityConfig.TrySetId(entity, entityId);
			if(revision != null)
				entityConfig.SetRevision(entity, revision);

			return entity;
		}

		public static object TryDeserialize(JObject document, IEntityConfig entityConfig)
		{
			return null;
		}

		public static JObject Serialize(object entity, IEntityConfig entityConfig)
		{
			if (entity == null)
				throw new ArgumentNullException("entity");

			if (entityConfig == null)
				throw new ArgumentNullException("entityConfig");

			if (entityConfig.EntityType != entity.GetType())
				throw new InvalidOperationException(string.Format(
					"Serializing type {0} does not match type {1} in configuration.", entity.GetType(), entityConfig.EntityType));

			var entityId = entityConfig.GetId(entity);
			if (string.IsNullOrWhiteSpace(entityId))
				throw new ArgumentException("Document ID should be set prior calling deserializer.", "entity");
			
			var documentId = entityConfig.ConvertEntityIdToDocumentId(entityId);
			if (string.IsNullOrWhiteSpace(documentId))
				throw new InvalidOperationException(
					"IEntityConfig.ConvertEntityIdToDocumentId() should not ever return null, empty or whitespace string.");

			var documentRevision = entityConfig.GetRevision(entity);

			var documentType = entityConfig.DocumentType;
			if (string.IsNullOrWhiteSpace(documentType))
				throw new InvalidOperationException(
					"IEntityConfig.DocumentType should not ever be null, empty or whitespace string.");
			
			var document = SerializeToJObject(entity, entityConfig);
			SetStandardPropertiesOnDocument(document, documentRevision, documentType, documentId);
			return document;
		}

		private static void GetDocumentTypeAndRevision(dynamic document, out string documentId, out string revision)
		{
			documentId = document._id;
			revision = document._rev;
		}

		private static object DeserializeFromJObject(JObject document, IEntityConfig entityConfig)
		{
			var serializer = Serializers.GetOrAdd(entityConfig, CreateSerializer);
			using (var jTokenReader = new JTokenReader(document))
				return serializer.Deserialize(jTokenReader, entityConfig.EntityType);
		}

		private static JObject SerializeToJObject(object entity, IEntityConfig entityConfig)
		{
			var serializer = Serializers.GetOrAdd(entityConfig, CreateSerializer);
			JObject document;
			using (var jTokenWriter = new JTokenWriter())
			{
				serializer.Serialize(jTokenWriter, entity);
				document = (JObject) jTokenWriter.Token;
			}
			return document;
		}

		private static void SetStandardPropertiesOnDocument(dynamic document, string revision, string type, string id)
		{
			document._id = id;
			if (!string.IsNullOrWhiteSpace(revision))
				document._rev = revision;
			document.type = type;
		}
		
		private static JsonSerializer CreateSerializer(IEntityConfig entityConfig)
		{
			var contractResolver = new ContractResolver(entityConfig.EntityType, entityConfig.IgnoredMembers);
			var settings = new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = contractResolver,
				Converters = { new IsoDateTimeConverter(), new StringEnumConverter() }
			};
			return JsonSerializer.Create(settings);
		}

		class ContractResolver : CamelCasePropertyNamesContractResolver
		{
			private readonly Type entityType;
			private readonly ISet<MemberInfo> ignoredMembers;

			public ContractResolver(Type entityType, IEnumerable<MemberInfo> ignoredMembers)
			{
				this.entityType = entityType;
				this.ignoredMembers = new HashSet<MemberInfo>(ignoredMembers);
			}

			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var jsonProperty = base.CreateProperty(member, memberSerialization);

				if (jsonProperty.PropertyType == entityType)
					throw new InvalidOperationException(
						string.Format(
							"Entity {0} references (including indirect ones) itself. This configuration is unsupported by CouchDude yet.",
							entityType.AssemblyQualifiedName));

				if (ignoredMembers.Contains(member))
					jsonProperty.Ignored = true;

				if (!jsonProperty.Writable)
				{
					var propertyInfo = member as PropertyInfo;
					if (propertyInfo != null)
					{
						var hasPrivateSetter = propertyInfo.GetSetMethod(true) != null;
						jsonProperty.Writable = hasPrivateSetter;
					}
				}

				return jsonProperty;
			}
		}
	}
}
