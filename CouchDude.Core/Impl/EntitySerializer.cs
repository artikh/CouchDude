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

			document = (JObject)document.DeepClone();

			string documentId, revision, documentType;
			GetDocumentTypeAndRevision(document, out documentId, out revision, out documentType);

			if (entityConfig.DocumentType != documentType)
				throw new InvalidOperationException(string.Format(
					"Deserializing document's type {0} does not match type {1} in configuration.", documentType, entityConfig.DocumentType));
			
			if(string.IsNullOrWhiteSpace(documentId))
				throw new DocumentIdMissingException(document);

			if(string.IsNullOrWhiteSpace(revision))
                throw new DocumentRevisionMissingException(document);

			var entityId = entityConfig.ConvertDocumentIdToEntityId(documentId);
			if(string.IsNullOrWhiteSpace(entityId))
				throw new InvalidOperationException(
					"IEntityConfig.ConvertDocumentIdToEntityId() should not ever return null, empty or whitespace string.");

			return DeserializeInternal(document, entityConfig, entityId, revision);
		}

		public static object TryDeserialize(JObject document, IEntityConfig entityConfig)
		{
			if (document == null)
				throw new ArgumentNullException("document");
			if (entityConfig == null)
				throw new ArgumentNullException("entityConfig");

			document = (JObject)document.DeepClone();

			string documentId, revision, documentType;
			GetDocumentTypeAndRevision(document, out documentId, out revision, out documentType);

			if (entityConfig.DocumentType == documentType)
			{
				var entityId = entityConfig.ConvertDocumentIdToEntityId(documentId);
				if (!string.IsNullOrWhiteSpace(entityId))
					return DeserializeInternal(document, entityConfig, entityId, revision);
			}
			return null;
		}

		private static object DeserializeInternal(JObject document, IEntityConfig entityConfig, string entityId, string revision)
		{
			var entity = DeserializeFromJObject(document, entityConfig);
			entityConfig.SetId(entity, entityId);
			if (revision != null)
				entityConfig.SetRevision(entity, revision);
			return entity;
		}

		public static JObject Serialize(object entity, IEntityConfig entityConfig, string previousRevisionValue = null)
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

			var documentRevision = 
				entityConfig.IsRevisionPresent
				? entityConfig.GetRevision(entity)
				: previousRevisionValue;

			var documentType = entityConfig.DocumentType;
			if (string.IsNullOrWhiteSpace(documentType))
				throw new InvalidOperationException(
					"IEntityConfig.DocumentType should not ever be null, empty or whitespace string.");
			
			var document = SerializeToJObject(entity, entityConfig);
			SetStandardPropertiesOnDocument(document, documentRevision, documentType, documentId);
			return document;
		}

		private static void GetDocumentTypeAndRevision(JObject document, out string documentId, out string revision, out string type)
		{
			documentId = document.Value<string>(IdPropertyName);
			revision = document.Value<string>(RevisionPropertyName);

			var typeProperty = document.Property(TypePropertyName);
			type = typeProperty.Value.Value<string>();
			typeProperty.Remove();
		}

		private static object DeserializeFromJObject(JObject document, IEntityConfig entityConfig)
		{
			var serializer = GetSerializer(entityConfig);
			using (var jTokenReader = new JTokenReader(document))
				return serializer.Deserialize(jTokenReader, entityConfig.EntityType);
		}

		private static JObject SerializeToJObject(object entity, IEntityConfig entityConfig)
		{
			var serializer = GetSerializer(entityConfig);
			JObject document;
			using (var jTokenWriter = new JTokenWriter())
			{
				serializer.Serialize(jTokenWriter, entity);
				document = (JObject) jTokenWriter.Token;
			}
			return document;
		}

		private static void SetStandardPropertiesOnDocument(JObject document, string revision, string type, string id)
		{
			document.AddFirst(new JProperty(TypePropertyName, JToken.FromObject(type)));
			if (!string.IsNullOrWhiteSpace(revision))
				document.AddFirst(new JProperty(RevisionPropertyName, JToken.FromObject(revision)));
			document.AddFirst(new JProperty(IdPropertyName, JToken.FromObject(id)));
		}

		private static JsonSerializer GetSerializer(IEntityConfig entityConfig)
		{
			return Serializers.GetOrAdd(entityConfig, CreateSerializer);
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
							"Entity {0} references itself (maybe indirectly). This configuration is unsupported by CouchDude yet.",
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
