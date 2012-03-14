using System;
using CouchDude.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CouchDude.Api
{
	/// <summary>Netonsoft Json.NET <see cref="DateTime"/>/<see cref="DateTimeOffset"/> converter saving date/time using ISO 8601 format.
	/// If fed with relative (non-UTC) time should convert it to UTC.</summary>
	public class IsoUtcDateTimeConverter : DateTimeConverterBase
	{
		private const string Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
		/// <inheritdoc/>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is DateTime)
				WriteDateTime(writer, value);
			else if (value is DateTimeOffset)
				WriteDateTimeOffset(writer, value);
			else
				throw new Exception(
					string.Format(
						"Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}.",
						value == null ? "<null>" : value.GetType().AssemblyQualifiedName));
		}

		private static void WriteDateTimeOffset(JsonWriter writer, object value)
		{
			var dateTimeOffset = (DateTimeOffset) value;
			dateTimeOffset = dateTimeOffset.ToUniversalTime();
			writer.WriteValue(dateTimeOffset.ToString(Format));
		}

		private static void WriteDateTime(JsonWriter writer, object value)
		{
			var dateTime = (DateTime) value;
			switch (dateTime.Kind)
			{
				case DateTimeKind.Local:
					dateTime = dateTime.ToUniversalTime();
					break;
				case DateTimeKind.Unspecified: // if unspecified assuming UTC
					dateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
					break;
				case DateTimeKind.Utc:
					break;
			}
			writer.WriteValue(dateTime.ToString(Format));
		}

		/// <inheritdoc/>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var isNullable = objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
			var actualType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

			if (reader.TokenType == JsonToken.Null)
			{
				if (isNullable)
					return null;
				throw new ParseException("Cannot convert JSON null value to {0}.", objectType);
			}

			if (reader.TokenType == JsonToken.String)
			{
				var dateTimeOffset = FlexibleIso8601DateTimeParser.TryParse(reader.Value.ToString());

				if (dateTimeOffset == null)
					return isNullable? Activator.CreateInstance(actualType): null;

				if (actualType == typeof (DateTimeOffset))
					return dateTimeOffset.Value;

				if (actualType == typeof(DateTime))
					return dateTimeOffset.Value.UtcDateTime;

				throw new ParseException("Unexpected target type {0}.", actualType);
			}

			throw new ParseException("Unexpected token parsing date. Expected String, got {0}.", reader.TokenType);
		}
	}
}