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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using CouchDude.Configuration;
using CouchDude.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Api
{
	public partial class Document
	{
		private static readonly ConcurrentDictionary<IEntityConfig, JsonSerializer> Serializers =
			new ConcurrentDictionary<IEntityConfig, JsonSerializer>();
		
		/// <summary>Deserializes document to new entity object.</summary>
		/// <param name="entityConfig">Entity configuration used to deserialize it properly.</param>
		public object Deserialize(IEntityConfig entityConfig)
		{
			if (entityConfig == null)
				throw new ArgumentNullException("entityConfig");

			return CheckAndDeserialize(entityConfig, JsonObject, this);
		}

		/// <summary>Deserializes document to new entity object returning <c>null</c> insted of exception if
		/// it is impossible.</summary>
		/// <param name="entityConfig">Entity configuration used to deserialize it properly.</param>
		public object TryDeserialize(IEntityConfig entityConfig)
		{
			if (entityConfig == null)
				throw new ArgumentNullException("entityConfig");

			var document = (JObject)JsonObject.DeepClone();
			string documentId, revision, documentType;
			GetDocumentTypeAndRevision(document, out documentId, out revision, out documentType);

			if (entityConfig.DocumentType == documentType)
			{
				var entityId = entityConfig.ConvertDocumentIdToEntityId(documentId);
				if (!String.IsNullOrWhiteSpace(entityId))
					return DeserializeInternal(document, entityConfig, entityId, revision);
			}
			return null;
		}

		/// <summary>Serializes entity in simple mode not using configuration settings. Entity required 
		/// to have _id  property.</summary>
		public new static IDocument Serialize(object entity)
		{
			JObject jObject;
			using (var jTokenWriter = new JTokenWriter())
			{
				Serializer.Serialize(jTokenWriter, entity);
				jObject = (JObject)jTokenWriter.Token;
			}

			var document = new Document(jObject);
			if (document.Id.HasNoValue())
				throw new ArgumentOutOfRangeException(
					"entity", "Entity _id property should be set prior serializing to document.");
			return document;
		}

		/// <summary>Serializes entity using provided <paramref name="entityConfig"/> producing
		/// new <see cref="Document"/> instance.</summary>
		public static IDocument Serialize(object entity, IEntityConfig entityConfig)
		{
			if (entity == null)
				throw new ArgumentNullException("entity");
			if (entityConfig == null)
				throw new ArgumentNullException("entityConfig");

			if (entityConfig.EntityType != entity.GetType())
				throw new InvalidOperationException(String.Format(
					"Serializing type {0} does not match type {1} in configuration.", entity.GetType(), entityConfig.EntityType));

			var entityId = entityConfig.GetId(entity);
			if (String.IsNullOrWhiteSpace(entityId))
				throw new ArgumentException("Entity ID should be set prior calling deserializer.", "entity");

			var documentId = entityConfig.ConvertEntityIdToDocumentId(entityId);
			if (String.IsNullOrWhiteSpace(documentId))
				throw new InvalidOperationException(
					"IEntityConfig.ConvertEntityIdToDocumentId() should not ever return null, empty or whitespace string.");

			var documentRevision =
				entityConfig.IsRevisionPresent
					? entityConfig.GetRevision(entity)
					: null;

			var documentType = entityConfig.DocumentType;
			if (String.IsNullOrWhiteSpace(documentType))
				throw new InvalidOperationException(
					"IEntityConfig.DocumentType should not ever be null, empty or whitespace string.");

			var document = SerializeToJObject(entity, entityConfig);
			SetStandardPropertiesOnDocument(document, documentRevision, documentType, documentId);
			var jsonDocument = document;

			return new Document(jsonDocument);
		}

		private static object CheckAndDeserialize(IEntityConfig entityConfig, JObject jsonObject, Document document)
		{
			var clonedJObject = (JObject) jsonObject.DeepClone();

			string documentId, revision, documentType;
			GetDocumentTypeAndRevision(clonedJObject, out documentId, out revision, out documentType);

			if(documentType.HasNoValue())
				throw new DocumentTypeMissingException(document);

			if (entityConfig.DocumentType != documentType)
				throw new InvalidOperationException(String.Format(
					"Deserializing document's type {0} does not match type {1} in configuration.", documentType,
					entityConfig.DocumentType));

			if (String.IsNullOrWhiteSpace(documentId))
				throw new DocumentIdMissingException(document);

			if (String.IsNullOrWhiteSpace(revision))
				throw new DocumentRevisionMissingException(document);

			var entityId = entityConfig.ConvertDocumentIdToEntityId(documentId);
			if (String.IsNullOrWhiteSpace(entityId))
				throw new InvalidOperationException(
					"IEntityConfig.ConvertDocumentIdToEntityId() should not ever return null, empty or whitespace string.");

			return DeserializeInternal(clonedJObject, entityConfig, entityId, revision);
		}

		private static object DeserializeInternal(JObject document, IEntityConfig entityConfig, string entityId, string revision)
		{
			var entity = DeserializeFromJObject(document, entityConfig);
			entityConfig.SetId(entity, entityId);
			if (revision != null)
				entityConfig.SetRevision(entity, revision);
			return entity;
		}

		private static void GetDocumentTypeAndRevision(JObject document, out string documentId, out string revision, out string type)
		{
			documentId = document.Value<string>(IdPropertyName);
			revision = document.Value<string>(RevisionPropertyName);

			var typeProperty = document.Property(TypePropertyName);
			if (typeProperty != null)
			{
				type = typeProperty.Value.Value<string>();
				typeProperty.Remove();
			}
			else
				type = null;
		}

		private static object DeserializeFromJObject(JObject document, IEntityConfig entityConfig)
		{
			var serializer = GetSerializer(entityConfig);
			using (var jTokenReader = new JTokenReader(document))
				try
				{	
					return serializer.Deserialize(jTokenReader, entityConfig.EntityType);
				}
				catch (JsonSerializationException e)
				{
					throw new ParseException(e, "Error deserialising document");
				}
		}

		private static JObject SerializeToJObject(object entity, IEntityConfig entityConfig)
		{
			var serializer = GetSerializer(entityConfig);
			JObject document;
			using (var jTokenWriter = new JTokenWriter())
			{
				serializer.Serialize(jTokenWriter, entity);
				document = (JObject)jTokenWriter.Token;
			}
			return document;
		}

		private static void SetStandardPropertiesOnDocument(JContainer document, string revision, string type, string id)
		{
			document.AddFirst(new JProperty(TypePropertyName, JToken.FromObject(type)));
			if (!String.IsNullOrWhiteSpace(revision))
				document.AddFirst(new JProperty(RevisionPropertyName, JToken.FromObject(revision)));
			document.AddFirst(new JProperty(IdPropertyName, JToken.FromObject(id)));
		}

		private static JsonSerializer GetSerializer(IEntityConfig entityConfig)
		{
			return Serializers.GetOrAdd(entityConfig, CreateSerializer);
		}

		private static JsonSerializer CreateSerializer(IEntityConfig entityConfig)
		{
			var settings = CreateSerializerSettings();
			settings.ContractResolver = new EntityContractResolver(entityConfig.EntityType, entityConfig.IgnoredMembers);
			return JsonSerializer.Create(settings);
		}

		class EntityContractResolver : CamelCasePrivateSetterPropertyContractResolver
		{
			private readonly Type entityType;
			private readonly ISet<MemberInfo> ignoredMembers;

			public EntityContractResolver(Type entityType, IEnumerable<MemberInfo> ignoredMembers)
			{
				this.entityType = entityType;
				this.ignoredMembers = new HashSet<MemberInfo>(ignoredMembers);
			}

			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var jsonProperty = base.CreateProperty(member, memberSerialization);
				
				if (jsonProperty.PropertyType == entityType)
					throw new InvalidOperationException(
						String.Format(
							"Entity {0} references itself (maybe indirectly). This configuration is unsupported by CouchDude yet.",
							entityType.AssemblyQualifiedName));

				if (ignoredMembers.Contains(member))
					jsonProperty.Ignored = true;

				return jsonProperty;
			}
		}
	}
}