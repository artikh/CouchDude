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
using System.IO;
using System.Json;
using System.Threading;
using CouchDude.Configuration;
using CouchDude.Utils;
using Newtonsoft.Json;

namespace CouchDude.Serialization
{
	/// <summary><see cref="Newtonsoft.Json"/>-based serializer.</summary>
	public class NewtonsoftSerializer : SerializerBase
	{
		readonly Lazy<JsonSerializer> defaultSerializer;
		readonly LazyConcurrentDictionary<IEntityConfig, JsonSerializer> entitySerializers;

		/// <constructor />
		public NewtonsoftSerializer(
			Func<JsonSerializerSettings> createDefaultSerializerSettings,
			Func<IEntityConfig, JsonSerializerSettings> createEntitySerializerSettings)
		{
			createDefaultSerializerSettings =
				createDefaultSerializerSettings ?? NewtonsoftSerializerDefautSettings.CreateDefaultSerializerSettingsDefault;
			createEntitySerializerSettings =
				createEntitySerializerSettings ?? NewtonsoftSerializerDefautSettings.CreateEntitySerializerSettingsDefault;

			defaultSerializer = new Lazy<JsonSerializer>(
				() => JsonSerializer.Create(createDefaultSerializerSettings()), LazyThreadSafetyMode.PublicationOnly);
			entitySerializers = new LazyConcurrentDictionary<IEntityConfig, JsonSerializer>(
				entityConfig => JsonSerializer.Create(createEntitySerializerSettings(entityConfig)));
		}
		
		public override void Serialize(TextWriter target, object source, bool throwOnError = false)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (source == null) throw new ArgumentNullException("source");
			try
			{
				defaultSerializer.Value.Serialize(target, source);
			}
			catch (JsonSerializationException e)
			{
				LogAndThrowParseExceptionIfNeeded<object>(
					throwOnError, e, "Error serializing object of type {0}", source.GetType().FullName);
			}
		}

		public override object Deserialize(Type targetType, TextReader source, bool throwOnError = false)
		{
			if (targetType == null) throw new ArgumentNullException("targetType");
			if (source == null) throw new ArgumentNullException("source");

			try 
			{
				return defaultSerializer.Value.Deserialize(source, targetType);
			}
			catch (JsonSerializationException e)
			{
				return LogAndThrowParseExceptionIfNeeded<object>(
					throwOnError, e, "Error deserializing object of type {0}", source.GetType().FullName);
			}
		}

		public override JsonValue ConvertToJson(object source, bool throwOnError = false)
		{
			if (source == null) throw new ArgumentNullException("source");
			return ConvertToJsonInternal(source, defaultSerializer.Value, throwOnError);
		}

		public override object ConvertFromJson(Type targetType, JsonValue source, bool throwOnError = false)
		{
			if (targetType == null) throw new ArgumentNullException("targetType");
			if (source == null) throw new ArgumentNullException("source");
			return ConvertFromJsonInternal(targetType, source, defaultSerializer.Value, throwOnError);
		}

		public override JsonObject ConvertToJson(object sourceEntity, IEntityConfig entityConfig, bool throwOnError = false)
		{
			if (sourceEntity == null) throw new ArgumentNullException("sourceEntity");
			if (entityConfig == null) throw new ArgumentNullException("entityConfig");

			return CheckAndConvertToJson(
				sourceEntity,
				entityConfig,
				throwOnError,
				() => (JsonObject)ConvertToJsonInternal(sourceEntity, entitySerializers.Get(entityConfig), throwOnError)
			);
		}

		public override object ConvertFromJson(IEntityConfig entityConfig, JsonObject source, bool throwOnError = false)
		{
			if (entityConfig == null) throw new ArgumentNullException("entityConfig");
			if (source == null) throw new ArgumentNullException("source");

			return CheckAndConvertFromJson(
				entityConfig, 
				source, 
				throwOnError, 
				doc => ConvertFromJsonInternal(entityConfig.EntityType, doc, entitySerializers.Get(entityConfig), throwOnError)
			);
		}

		private static JsonValue ConvertToJsonInternal(object source, JsonSerializer serializer, bool throwOnError)
		{
			using (var writer = new SystemJsonValueWriter())
				try
				{
					serializer.Serialize(writer, source);
					return writer.JsonValue;
				}
				catch (JsonSerializationException e)
				{
					return LogAndThrowParseExceptionIfNeeded<JsonValue>(
						throwOnError, e, "Error converting object of type {0} to JSON", source.GetType().FullName);
				}
		}

		private static object ConvertFromJsonInternal(
			Type targetType, JsonValue source, JsonSerializer serializer, bool throwOnError)
		{
			using (var reader = new SystemJsonValueReader(source))
				try
				{
					return serializer.Deserialize(reader, targetType);
				}
				catch (JsonSerializationException e)
				{
					return LogAndThrowParseExceptionIfNeeded<object>(
						throwOnError, e, "Error converting JSON to object of provided type {0}", targetType);
				}
		}
	}
}