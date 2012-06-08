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
			if (rootValue == null) throw new ArgumentNullException("rootValue");
			if (rootValue.JsonType == JsonType.Default)
				throw new ArgumentException("'Default' JSON values could not be read.", "rootValue");

			this.rootValue = rootValue;
		}
		
		/// <summary>Reads the next JSON token from the stream.</summary>
		/// <returns>true if the next token was read successfully; false if there are no more tokens to read.</returns>
		public override bool Read()
		{
			return ReadInternal();
		}

		/// <inheritdoc />
		public override byte[] ReadAsBytes()
		{
			return ReadAsBytesInternal();
		}

		/// <inheritdoc />
		public override decimal? ReadAsDecimal()
		{
			return ReadAsDecimalInternal();
		}

		/// <inheritdoc />
		public override int? ReadAsInt32()
		{
			return ReadAsInt32Internal();
		}

		/// <inheritdoc />
		public override string ReadAsString()
		{
			return ReadAsStringInternal();
		}

		/// <inheritdoc />
		public override DateTime? ReadAsDateTime()
		{
			return ReadAsDateTimeInternal();
		}

		/// <inheritdoc />
		public override DateTimeOffset? ReadAsDateTimeOffset()
		{
			return ReadAsDateTimeOffsetInternal();
		}


		internal bool ReadInternal()
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

		internal DateTimeOffset? ReadAsDateTimeOffsetInternal()
		{
			do
			{
				if (!ReadInternal())
				{
					SetToken(JsonToken.None);
					return null;
				}
			} while (TokenType == JsonToken.Comment);

			if (TokenType == JsonToken.Date)
			{
				if (Value is DateTime)
					SetToken(JsonToken.Date, new DateTimeOffset((DateTime)Value));

				return (DateTimeOffset)Value;
			}

			if (TokenType == JsonToken.Null)
				return null;

			if (TokenType == JsonToken.String)
			{
				DateTimeOffset dt;
				if (DateTimeOffset.TryParse((string)Value, Culture, DateTimeStyles.RoundtripKind, out dt))
				{
					SetToken(JsonToken.Date, dt);
					return dt;
				}
				else
				{
					throw CreateExeception(this, string.Format("Could not convert string to DateTimeOffset: {0}.", CultureInfo.InvariantCulture));
				}
			}

			if (TokenType == JsonToken.EndArray)
				return null;

			throw CreateExeception(this, string.Format("Error reading date. Unexpected token: {0}.", CultureInfo.InvariantCulture));
		}

		internal byte[] ReadAsBytesInternal()
		{
			do
			{
				if (!ReadInternal())
				{
					SetToken(JsonToken.None);
					return null;
				}
			} while (TokenType == JsonToken.Comment);

			if (IsWrappedInTypeObject())
			{
				byte[] data = ReadAsBytes();
				ReadInternal();
				SetToken(JsonToken.Bytes, data);
				return data;
			}

			// attempt to convert possible base 64 string to bytes
			if (TokenType == JsonToken.String)
			{
				var s = (string)Value;
				var data = (s.Length == 0) ? new byte[0] : Convert.FromBase64String(s);
				SetToken(JsonToken.Bytes, data);
			}

			if (TokenType == JsonToken.Null)
				return null;

			if (TokenType == JsonToken.Bytes)
				return (byte[])Value;

			if (TokenType == JsonToken.StartArray)
			{
				var data = new List<byte>();

				while (ReadInternal())
				{
					switch (TokenType)
					{
						case JsonToken.Integer:
							data.Add(Convert.ToByte(Value, CultureInfo.InvariantCulture));
							break;
						case JsonToken.EndArray:
							byte[] d = data.ToArray();
							SetToken(JsonToken.Bytes, d);
							return d;
						case JsonToken.Comment:
							// skip
							break;
						default:
							throw CreateExeception(this, string.Format("Unexpected token when reading bytes: {0}.", CultureInfo.InvariantCulture));
					}
				}

				throw CreateExeception(this, "Unexpected end when reading bytes.");
			}

			if (TokenType == JsonToken.EndArray)
				return null;

			throw CreateExeception(this, string.Format("Error reading bytes. Unexpected token: {0}.", CultureInfo.InvariantCulture));
		}

		internal decimal? ReadAsDecimalInternal()
		{
			do
			{
				if (!ReadInternal())
				{
					SetToken(JsonToken.None);
					return null;
				}
			} while (TokenType == JsonToken.Comment);

			if (TokenType == JsonToken.Integer || TokenType == JsonToken.Float)
			{
				if (!(Value is decimal))
					SetToken(JsonToken.Float, Convert.ToDecimal(Value, CultureInfo.InvariantCulture));

				return (decimal)Value;
			}

			if (TokenType == JsonToken.Null)
				return null;

			if (TokenType == JsonToken.String)
			{
				decimal d;
				if (Decimal.TryParse((string)Value, NumberStyles.Number, Culture, out d))
				{
					SetToken(JsonToken.Float, d);
					return d;
				}
				else
				{
					throw CreateExeception(this, string.Format("Could not convert string to decimal: {0}.", CultureInfo.InvariantCulture));
				}
			}

			if (TokenType == JsonToken.EndArray)
				return null;

			throw CreateExeception(this, string.Format("Error reading decimal. Unexpected token: {0}.", CultureInfo.InvariantCulture));
		}

		internal int? ReadAsInt32Internal()
		{
			do
			{
				if (!ReadInternal())
				{
					SetToken(JsonToken.None);
					return null;
				}
			} while (TokenType == JsonToken.Comment);

			if (TokenType == JsonToken.Integer || TokenType == JsonToken.Float)
			{
				if (!(Value is int))
					SetToken(JsonToken.Integer, Convert.ToInt32(Value, CultureInfo.InvariantCulture));

				return (int)Value;
			}

			if (TokenType == JsonToken.Null)
				return null;

			if (TokenType == JsonToken.String)
			{
				int i;
				if (Int32.TryParse((string)Value, NumberStyles.Integer, Culture, out i))
				{
					SetToken(JsonToken.Integer, i);
					return i;
				}
				else
				{
					throw CreateExeception(this, string.Format("Could not convert string to integer: {0}.", CultureInfo.InvariantCulture));
				}
			}

			if (TokenType == JsonToken.EndArray)
				return null;

			throw CreateExeception(this, string.Format("Error reading integer. Unexpected token: {0}.", CultureInfo.InvariantCulture));
		}

		internal string ReadAsStringInternal()
		{
			do
			{
				if (!ReadInternal())
				{
					SetToken(JsonToken.None);
					return null;
				}
			} while (TokenType == JsonToken.Comment);

			if (TokenType == JsonToken.String)
				return (string)Value;

			if (TokenType == JsonToken.Null)
				return null;

			if (IsPrimitiveToken(TokenType))
			{
				if (Value != null)
				{
					string s;
					var convertible = Value as IConvertible;
					if (convertible != null)
						s = convertible.ToString(Culture);
					else if (Value is IFormattable)
						s = ((IFormattable)Value).ToString(null, Culture);
					else
						s = Value.ToString();

					SetToken(JsonToken.String, s);
					return s;
				}
			}

			if (TokenType == JsonToken.EndArray)
				return null;

			throw CreateExeception(this, string.Format("Error reading string. Unexpected token: {0}.", CultureInfo.InvariantCulture));
		}

		internal DateTime? ReadAsDateTimeInternal()
		{
			do
			{
				if (!ReadInternal())
				{
					SetToken(JsonToken.None);
					return null;
				}
			} while (TokenType == JsonToken.Comment);

			if (TokenType == JsonToken.Date)
				return (DateTime)Value;

			if (TokenType == JsonToken.Null)
				return null;

			if (TokenType == JsonToken.String)
			{
				var s = (string)Value;
				if (String.IsNullOrEmpty(s))
				{
					SetToken(JsonToken.Null);
					return null;
				}

				DateTime dt;
				if (DateTime.TryParse(s, Culture, DateTimeStyles.RoundtripKind, out dt))
				{
					dt = EnsureDateTime(dt, DateTimeZoneHandling);
					SetToken(JsonToken.Date, dt);
					return dt;
				}
				else
				{
					throw CreateExeception(this, string.Format("Could not convert string to DateTime: {0}.", CultureInfo.InvariantCulture));
				}
			}

			if (TokenType == JsonToken.EndArray)
				return null;

			throw CreateExeception(this, string.Format("Error reading date. Unexpected token: {0}.", CultureInfo.InvariantCulture));
		}


		private static DateTime SwitchToLocalTime(DateTime value)
		{
			switch (value.Kind)
			{
				case DateTimeKind.Unspecified:
					return new DateTime(value.Ticks, DateTimeKind.Local);

				case DateTimeKind.Utc:
					return value.ToLocalTime();

				case DateTimeKind.Local:
					return value;
			}
			return value;
		}

		private static DateTime SwitchToUtcTime(DateTime value)
		{
			switch (value.Kind)
			{
				case DateTimeKind.Unspecified:
					return new DateTime(value.Ticks, DateTimeKind.Utc);

				case DateTimeKind.Utc:
					return value;

				case DateTimeKind.Local:
					return value.ToUniversalTime();
			}
			return value;
		}

		static DateTime EnsureDateTime(DateTime value, DateTimeZoneHandling timeZone)
		{
			switch (timeZone)
			{
				case DateTimeZoneHandling.Local:
					value = SwitchToLocalTime(value);
					break;
				case DateTimeZoneHandling.Utc:
					value = SwitchToUtcTime(value);
					break;
				case DateTimeZoneHandling.Unspecified:
					value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
					break;
				case DateTimeZoneHandling.RoundtripKind:
					break;
				default:
					throw new ArgumentException("Invalid date time handling value.");
			}

			return value;
		}

		static bool IsPrimitiveToken(JsonToken token)
		{
			switch (token)
			{
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.String:
				case JsonToken.Boolean:
				case JsonToken.Undefined:
				case JsonToken.Null:
				case JsonToken.Date:
				case JsonToken.Bytes:
					return true;
				default:
					return false;
			}
		}

		bool IsWrappedInTypeObject()
		{
			if (TokenType == JsonToken.StartObject)
			{
				if (!ReadInternal())
					throw CreateExeception(this, "Unexpected end when reading bytes.");

				if (Value.ToString() == "$type")
				{
					ReadInternal();
					if (Value != null && Value.ToString().StartsWith("System.Byte[]"))
					{
						ReadInternal();
						if (Value.ToString() == "$value")
						{
							return true;
						}
					}
				}

				throw CreateExeception(this, string.Format("Error reading bytes. Unexpected token: {0}.", CultureInfo.InvariantCulture));
			}

			return false;
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

		static IEnumerable<EmitValue> EnumerateObjectIfNotNull(IEnumerable<KeyValuePair<string, JsonValue>> jsonObject)
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
						primitiveValue.Value is int? JsonToken.Integer: JsonToken.Float, primitiveValue.Value);
					yield break;
				case JsonType.Boolean:
					yield return new EmitValue(JsonToken.Boolean, primitiveValue.Value);
					yield break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal static string FormatExceptionMessage(IJsonLineInfo lineInfo, string path, string message)
		{
			// don't add a fullstop and space when message ends with a new line
			if (!message.EndsWith(Environment.NewLine))
			{
				message = message.Trim();

				if (!message.EndsWith("."))
					message += ".";

				message += " ";
			}

			message += string.Format("Path '{0}'", path);

			if (lineInfo != null && lineInfo.HasLineInfo())
				message += string.Format(", line {0}, position {1}", lineInfo.LineNumber, lineInfo.LinePosition);

			message += ".";

			return message;
		}

		internal static JsonReaderException CreateExeception(JsonReader reader, string message)
		{
			var complexMessage = FormatExceptionMessage(reader as IJsonLineInfo, reader.Path, message);
			return new JsonReaderException(complexMessage);
		}
	}
}
