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
using System.Globalization;
using System.Json;
using System.Linq;
using Newtonsoft.Json;

namespace CouchDude.Utils
{
	/// <summary><see cref="JsonReader"/> implementation reading <see cref="JsonValue"/> object.</summary>
	public class SystemJsonValueReader : JsonReader
	{
		struct EmitValue
		{
			public readonly JsonToken TokenType;
			public readonly object Value;

			public EmitValue(JsonToken tokenType, object value = null)
			{
				TokenType = tokenType;
				Value = value;
			}
		}

		IEnumerator<EmitValue> jsonValueEnumerator;
		readonly JsonValue rootValue;
		
		/// <constructor />
		public SystemJsonValueReader(JsonValue rootValue)
		{
			if(rootValue == null) throw new ArgumentNullException("rootValue");
			if (rootValue.JsonType == JsonType.Default)
				throw new ArgumentException("'Default' JSON values could not be read.", "rootValue");

			this.rootValue = rootValue;
		}

		/// <inheritdoc />
		public override bool Read()
		{
			if (jsonValueEnumerator == null)
				jsonValueEnumerator = EnumerateJson(rootValue).GetEnumerator();

			if (jsonValueEnumerator.MoveNext())
			{
				SetToken(jsonValueEnumerator.Current.TokenType, jsonValueEnumerator.Current.Value);
				return true;
			}
			return false;
		}

        /// <inheritdoc />
        public override string ReadAsString() 
        {
            Read();
            SetToken(JsonToken.String, Value);
            return Value.ToString(); 
        }

	    /// <inheritdoc />
		public override byte[] ReadAsBytes() { throw new NotImplementedException(); }

		/// <inheritdoc />
		public override decimal? ReadAsDecimal() { throw new NotImplementedException(); }

        /// <inheritdoc />
        public override DateTime? ReadAsDateTime() { throw new NotImplementedException(); }

	    /// <inheritdoc />
		public override DateTimeOffset? ReadAsDateTimeOffset() { throw new NotImplementedException(); }

		/// <inheritdoc />
		public override int? ReadAsInt32() 
		{
			Read();
			if (TokenType == JsonToken.Integer || TokenType == JsonToken.Float)
			{
				SetToken(JsonToken.Integer, Convert.ToInt32(Value, CultureInfo.InvariantCulture));
				return (int)Value;
			}
			
			if (TokenType == JsonToken.Null)
				return new int?();
			
			if (TokenType == JsonToken.String)
			{
				int result;
				if (int.TryParse(
					(string)Value, 
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign, 
					Culture, 
					out result))
				{
					SetToken(JsonToken.Integer, result);
					return result;
				}

				throw new JsonSerializationException(String.Format("Could not convert string to integer: {0}.", Value));
			}

			throw new JsonSerializationException(String.Format("Error reading integer. Expected a number but got {0}.", TokenType));
		}
		
		static IEnumerable<EmitValue> EnumerateJson(JsonValue jsonValue)
		{
			return EnumerateNullIfNullItIs(jsonValue)
				.Concat(EnumerateArrayIfNotNull(jsonValue as JsonArray))
				.Concat(EnumerateObjectIfNotNull(jsonValue as JsonObject))
				.Concat(EnumeratePrimitiveIfNotNull(jsonValue as JsonPrimitive));
		}

		static IEnumerable<EmitValue> EnumerateNullIfNullItIs(JsonValue jsonValue)
		{
			if (jsonValue == null)
				yield return new EmitValue(JsonToken.Null);
		}

		static IEnumerable<EmitValue> EnumerateArrayIfNotNull(JsonArray jsonArray)
		{
			if (jsonArray == null) yield break;

			yield return new EmitValue(JsonToken.StartArray);

			foreach (var arrayItem in jsonArray)
				foreach (var arrayItemEmit in EnumerateJson(arrayItem))
					yield return arrayItemEmit;

			yield return new EmitValue(JsonToken.EndArray);
		}

		static IEnumerable<EmitValue> EnumerateObjectIfNotNull(JsonObject jsonObject)
		{
			if (jsonObject == null) yield break;

			yield return new EmitValue(JsonToken.StartObject);

			foreach (var property in jsonObject)
			{
				yield return new EmitValue(JsonToken.PropertyName, property.Key);
				foreach (var propertyValueEmit in EnumerateJson(property.Value))
					yield return propertyValueEmit;
			}

			yield return new EmitValue(JsonToken.EndObject);
		}

		static IEnumerable<EmitValue> EnumeratePrimitiveIfNotNull(JsonPrimitive primitiveValue)
		{
			if (primitiveValue == null) yield break;

			switch (primitiveValue.JsonType)
			{
				case JsonType.String:
					yield return new EmitValue(JsonToken.String, primitiveValue.Value);
					yield break;
				case JsonType.Number:
					yield return new EmitValue(
						primitiveValue.Value is int ? JsonToken.Integer : JsonToken.Float, primitiveValue.Value);
					yield break;
				case JsonType.Boolean:
					yield return new EmitValue(JsonToken.Boolean, primitiveValue.Value);
					yield break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
