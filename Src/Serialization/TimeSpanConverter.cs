using System;
using Newtonsoft.Json;

namespace CouchDude.Serialization
{
	/// <summary>Newtonsoft Json.NET <see cref="TimeSpan"/> converter.</summary>
	public class TimeSpanConverter : JsonConverter
	{
		/// <inheritdoc/>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
				writer.WriteNull();
			else
				writer.WriteValue(((TimeSpan)value).TotalMilliseconds);
		}

		/// <inheritdoc/>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Null:
					return default(TimeSpan);
				case JsonToken.Integer:
				case JsonToken.Float:
					var milliseconds = Convert.ToDouble(reader.Value);
					return TimeSpan.FromMilliseconds(milliseconds);
				default:
					throw new Exception(
						String.Format("Unexpected token when reading TimeSpan. Expected Integer or Float, got {0}.", reader.TokenType));
			}
		}

		/// <inheritdoc/>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(TimeSpan);
		}
	}
}