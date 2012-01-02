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
using System.Collections.Generic;
using System.IO;
using System.Json;
using Common.Logging;
using CouchDude.Configuration;
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude.Serialization
{
	/// <summary>Base class with utility methods for <see cref="ISerializer"/> implementations.</summary>
	public abstract class SerializerBase : ISerializer
	{
		private static readonly ILog Log = LogManager.GetCurrentClassLogger();
		
		/// <inheritdoc />
		public abstract void Serialize(
			[NotNull] TextWriter target, [NotNull] object source, bool throwOnError);

		/// <inheritdoc />
		public abstract object Deserialize([NotNull] Type targetType, [NotNull] TextReader source, bool throwOnError);

		/// <inheritdoc />
		public abstract JsonValue ConvertToJson([NotNull] object source, bool throwOnError);

		/// <inheritdoc />
		public abstract object ConvertFromJson(
			[NotNull] Type targetType, [NotNull] JsonValue source, bool throwOnError);

		/// <inheritdoc />
		public abstract JsonObject ConvertToJson(
			[NotNull] object sourceEntity, [NotNull] IEntityConfig entityConfig, bool throwOnError);

		/// <inheritdoc />
		public abstract object ConvertFromJson(
			[NotNull] IEntityConfig entityConfig, [NotNull] JsonObject source, bool throwOnError);

		/// <summary>Implements basic check and convertion logic for ConvertToJson method.</summary>
		protected static JsonObject CheckAndConvertToJson(
			object sourceEntity, IEntityConfig entityConfig, bool throwOnError, Func<JsonObject> convert)
		{
			if (entityConfig.EntityType != sourceEntity.GetType())
				return LogAndThrowInvalidOperationExceptionIfNeeded<JsonObject>(
					throwOnError,
					"Serializing type {0} does not match type {1} in configuration.",
					sourceEntity.GetType(),
					entityConfig.EntityType
				);

			var entityId = entityConfig.GetId(sourceEntity);
			if (String.IsNullOrWhiteSpace(entityId))
				return LogAndThrowInvalidOperationExceptionIfNeeded<JsonObject>(
					throwOnError, "Entity ID should be set prior calling deserializer.");

			var documentId = entityConfig.ConvertEntityIdToDocumentId(entityId);
			if (String.IsNullOrWhiteSpace(documentId))
				return LogAndThrowInvalidOperationExceptionIfNeeded<JsonObject>(
					throwOnError,
					"IEntityConfig.ConvertEntityIdToDocumentId() should not ever return null, empty or whitespace string.");

			var documentRevision = entityConfig.IsRevisionPresent
				? entityConfig.GetRevision(sourceEntity)
				: null;

			var documentType = entityConfig.DocumentType;
			if (String.IsNullOrWhiteSpace(documentType))
				return LogAndThrowInvalidOperationExceptionIfNeeded<JsonObject>(
					throwOnError, "IEntityConfig.DocumentType should not ever be null, empty or whitespace string.");

			var jsonObject = convert();
			if (jsonObject == null)
				return null;

			// The only way simple to place special properties at the top is to write object again
			// Should investigate this as potential perf botleneck.
			var properties = new List<KeyValuePair<string, JsonValue>>();
			properties.Add(new KeyValuePair<string, JsonValue>(Document.IdPropertyName, documentId));
			if (!string.IsNullOrWhiteSpace(documentRevision))
				properties.Add(new KeyValuePair<string, JsonValue>(Document.RevisionPropertyName, documentRevision));
			properties.Add(new KeyValuePair<string, JsonValue>(Document.TypePropertyName, documentType));
			properties.AddRange(jsonObject);

			return new JsonObject(properties);
		}

		/// <summary>Implements basic check and convertion logic for ConvertFromJson method.</summary>
		protected static object CheckAndConvertFromJson(
			IEntityConfig entityConfig, JsonObject source, bool throwOnError, Func<JsonObject, object> convert)
		{
			var documentObject = source.DeepClone();
			var documentId = documentObject.GetPrimitiveProperty<string>(Document.IdPropertyName);
			var revision = documentObject.GetPrimitiveProperty<string>(Document.RevisionPropertyName);

			var documentType = documentObject.GetPrimitiveProperty<string>(Document.TypePropertyName);
			if (documentType != null)
				documentObject.Remove(Document.TypePropertyName);

			if (documentType.HasNoValue())
				return LogAndThrowIfNeeded<object>(throwOnError, new DocumentTypeMissingException(documentObject.ToString()));

			if (entityConfig.DocumentType != documentType)
				return LogAndThrowInvalidOperationExceptionIfNeeded<object>(
					throwOnError,
					"Deserializing document's type {0} does not match type {1} in configuration.", documentType,
					entityConfig.DocumentType
				);

			if (string.IsNullOrWhiteSpace(documentId))
				return LogAndThrowIfNeeded<object>(throwOnError, new DocumentIdMissingException(documentObject.ToString()));

			if (string.IsNullOrWhiteSpace(revision))
				return LogAndThrowIfNeeded<object>(throwOnError, new DocumentRevisionMissingException(documentObject.ToString()));

			var entityId = entityConfig.ConvertDocumentIdToEntityId(documentId);
			if (string.IsNullOrWhiteSpace(entityId))
				return LogAndThrowInvalidOperationExceptionIfNeeded<object>(
					throwOnError,
					"IEntityConfig.ConvertDocumentIdToEntityId() should not ever return null, empty or whitespace string."
				);

			var entity = convert(documentObject);

			entityConfig.SetId(entity, entityId);
			entityConfig.SetRevision(entity, revision);
			return entity;
		}
		
		/// Logs error and throws exception if <paramref name="throwOnError"/> is <c>true</c>
		protected static T LogAndThrowParseExceptionIfNeeded<T>(
			bool throwOnError, Exception exception, string messageTemplate, params object[] messageParams)
		{
			var message = string.Format(messageTemplate, messageParams);
			Log.Error(message, exception);
			if (throwOnError)
				throw new ParseException(exception, message);
			else
				return default(T);
		}

		// ReSharper disable UnusedParameter.Local
		private static T LogAndThrowInvalidOperationExceptionIfNeeded<T>(
			bool throwOnError, string messageTemplate, params object[] messageParams)
		{
			var message = string.Format(messageTemplate, messageParams);
			Log.Error(message);
			if (throwOnError)
				throw new InvalidOperationException(message);
			else
				return default(T);
		}

		private static T LogAndThrowIfNeeded<T>(bool throwOnError, Exception exception)
		{
			Log.Error(exception.Message, exception);
			if (throwOnError)
				throw exception;
			else
				return default(T);
		}
		// ReSharper restore UnusedParameter.Local
	}
}