#region Licence Info 
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
using System.IO;
using System.Json;
using System.Linq;
using System.Reflection;
using System.Text;
using Common.Logging;
using CouchDude.Configuration;
using CouchDude.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Api.Serialization
{
	/// <summary>CouchDude main serializer interface. 
	/// Default (and only included) implementation is <see cref="NewtonsoftSerializer"/>.</summary>
	public interface ISerializer
	{
		/// <summary>Serializes provided object of provided type to provided to instance of <see cref="TextWriter"/>.</summary>
		void Serialize(TextWriter target, object source);

		/// <summary>Deserializes object of provided type from provided instance of <see cref="TextReader"/>.</summary>
		object Deserialize(Type targetType, TextReader source);

		/// <summary>Converts provided object of provided type to JSON.</summary>
		JsonValue ConvertToJson(object source);

		/// <summary>Converts provided entity using provided <see cref="IEntityConfig"/> to JSON.</summary>
		JsonObject ConvertToJson(object sourceEntity, IEntityConfig entityConfig);

		/// <summary>Converts provided JSON to object of provided type.</summary>
		object ConvertFromJson(Type targetType, JsonValue source);

		/// <summary>Converts provided JSON to entity using provided <see cref="IEntityConfig"/>.</summary>
		object ConvertFromJson(IEntityConfig entityConfig, JsonValue source);
	}

	/// <summary>Base class for serializer implementations.</summary>
	public abstract class SerializerBase: ISerializer
	{
		public abstract void Serialize(TextWriter target, object source);
		public abstract object Deserialize(Type targetType, TextReader source);
		public abstract JsonValue ConvertToJson(object source);
		public abstract object ConvertFromJson(Type targetType, JsonValue source);
		public abstract JsonObject ConvertToJson(object sourceEntity, IEntityConfig entityConfig);
		public abstract object ConvertFromJson(IEntityConfig entityConfig, JsonValue source);
	}

	public class NewtonsoftSerializer : ISerializer
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();
		/// <summary>Standard set of JSON value convertors.</summary>
		static readonly JsonConverter[] Converters =
			new JsonConverter[] {
				new IsoUtcDateTimeConverter(), new StringEnumConverter(), new StringEnumConverter(), new UriConverter()
			};

		static readonly JsonSerializer DefaultSerializer = JsonSerializer.Create(CreateSerializerSettings());
		static readonly ConcurrentDictionary<IEntityConfig, JsonSerializer> Serializers =
			new ConcurrentDictionary<IEntityConfig, JsonSerializer>();

		static JsonSerializerSettings CreateSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new JsonFragment.CamelCasePrivateSetterPropertyContractResolver(),
				Converters = Converters
			};
		}

		public void Serialize(TextWriter target, object source)
		{
			DefaultSerializer.Serialize(target, source);
		}

		public object Deserialize(Type targetType, TextReader source)
		{
			return DefaultSerializer.Deserialize(source, targetType);
		}

		public JsonValue ConvertToJson(object source)
		{
			return ConvertToJsonInternal(source, DefaultSerializer.Serialize);
		}

		public object ConvertFromJson(Type targetType, JsonValue source)
		{
			return ConvertFromJsonInternal(targetType, source, DefaultSerializer.Deserialize);
		}



		private static JsonValue ConvertToJsonInternal(object source, Action<JsonWriter, object> serializeAction)
		{
			using (var writer = new SystemJsonValueWriter())
			{
				serializeAction(writer, source);
				return writer.JsonValue;
			}
		}

		private static object ConvertFromJsonInternal(
			Type targetType, JsonValue source, Func<JsonReader, Type, object> deserializeAction)
		{
			using (var reader = new SystemJsonValueReader(source))
				return deserializeAction(reader, targetType);
		}

		/*
		/// <summary>Deserializes current <see cref="JsonFragment"/> to object of provided <paramref name="type"/>.</summary>
		public object Deserialize(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");


			using (var jTokenReader = new JTokenReader(JsonToken))
				try
				{
					return Serializer.Deserialize(jTokenReader, type);
				}
				catch (JsonSerializationException e)
				{
					throw new ParseException(e, "Error deserialising JSON fragment");
				}
		}

		/// <summary>Deserializes current <see cref="JsonFragment"/> to object of provided <paramref name="type"/> returning
		/// <c>null</c> if deserialization was unsuccessful..</summary>
		public object TryDeserialize(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");


			using (var jTokenReader = new JTokenReader(JsonToken))
				try
				{
					return Serializer.Deserialize(jTokenReader, type);
				}
				catch (JsonSerializationException)
				{
					return null;
				}
		}

		/// <summary>Serializes provided object to <see cref="JsonFragment"/>.</summary>
		public static IJsonFragment Serialize(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");


			JToken jsonToken;
			using (var jTokenWriter = new JTokenWriter())
			{
				Serializer.Serialize(jTokenWriter, obj);
				jsonToken = jTokenWriter.Token;
			}
			return new JsonFragment(jsonToken);
		}

		/// <summary>Creates standard serializen properties.</summary>
		protected internal static JsonSerializerSettings CreateSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new JsonFragment.CamelCasePrivateSetterPropertyContractResolver(),
				Converters = Converters
			};
		}

		/// <summary>Resolves private setter properties writable.</summary>
		public class CamelCasePrivateSetterPropertyContractResolver : CamelCasePropertyNamesContractResolver
		{
			/// <inheritdoc />
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var jsonProperty = base.CreateProperty(member, memberSerialization);

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

		/// <summary>Writes JSON string to provided text writer.</summary>
		public void WriteTo(TextWriter writer)
		{
			using (var jTokenWriter = new JsonTextWriter(writer) { CloseOutput = false })
				JsonToken.WriteTo(jTokenWriter, Converters);
		}


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
			var clonedJObject = (JObject)jsonObject.DeepClone();

			string documentId, revision, documentType;
			GetDocumentTypeAndRevision(clonedJObject, out documentId, out revision, out documentType);

			if (documentType.HasNoValue())
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
			settings.ContractResolver = new Document.EntityContractResolver(entityConfig.EntityType, entityConfig.IgnoredMembers);
			return JsonSerializer.Create(settings);
		}

		class EntityContractResolver : JsonFragment.CamelCasePrivateSetterPropertyContractResolver
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
		*/
	}
}
