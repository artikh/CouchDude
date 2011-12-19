using System;
using System.Collections.Generic;
using System.Json;

namespace CouchDude.Utils
{
	/// <summary><see cref="System.Json"/> types recoursive comparier.</summary>
	public class JsonObjectComparier: 
		IEqualityComparer<JsonObject>, IEqualityComparer<JsonValue>, IEqualityComparer<JsonArray>, IEqualityComparer<JsonPrimitive>
	{
		/// <summary>Compares two <see cref="JsonObject"/> instances returning wether one is equal to another.</summary>
		public bool Equals(JsonObject objectA, JsonObject objectB)
		{
			if (objectA.Count != objectB.Count)
				return false;

			var enumeratorA = objectA.GetEnumerator();
			var enumeratorB = objectB.GetEnumerator();

			while (enumeratorA.MoveNext())
			{
				enumeratorB.MoveNext();

				var currentB = enumeratorB.Current;
				var currentA = enumeratorA.Current;
				var equals = currentA.Key == currentB.Key && Equals(currentA.Value, currentB.Value);
				if (!equals)
					return false; // returning false on first mismatch
			}
			return true;
		}

		/// <summary>Returns a hash code for the specified <see cref="JsonObject"/> instance.</summary>
		public int GetHashCode(JsonObject obj)
		{
			var hashCode = Int32.MinValue;
			foreach (var property in obj)
			{
				hashCode ^= property.Key.GetHashCode();
				hashCode ^= GetHashCode(property.Value);
			}
			return hashCode;
		}

		/// <summary>Compares two <see cref="JsonPrimitive"/> instances returning wether one is equal to another.</summary>
		public bool Equals(JsonPrimitive primitiveA, JsonPrimitive primitiveB)
		{
			return Equals(primitiveA.Value, primitiveB.Value);
		}

		/// <summary>Returns a hash code for the specified <see cref="JsonPrimitive"/> instance.</summary>
		public int GetHashCode(JsonPrimitive primitive)
		{
			return primitive.Value.GetHashCode();
		}

		/// <summary>Compares two <see cref="JsonValue"/> instances returning wether one is equal to another.</summary>
		public bool Equals(JsonValue valueA, JsonValue valueB)
		{
			if (valueA.JsonType != valueB.JsonType)
				return false;

			switch (valueA.JsonType /* same as valueB.JsonType now*/)
			{
				case JsonType.String:
				case JsonType.Number:
				case JsonType.Boolean:
					return Equals((JsonPrimitive) valueA, (JsonPrimitive) valueB);
				case JsonType.Object:
					return Equals((JsonObject)valueA, (JsonObject)valueB);
				case JsonType.Array:
					return Equals((JsonArray)valueA, (JsonArray)valueB);
				case JsonType.Default:
					return true;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>Returns a hash code for the specified <see cref="JsonValue"/> instance.</summary>
		public int GetHashCode(JsonValue value)
		{
			switch (value.JsonType)
			{
				case JsonType.String:
				case JsonType.Number:
				case JsonType.Boolean:
					return GetHashCode((JsonPrimitive) value);
				case JsonType.Object:
					return GetHashCode((JsonObject)value);
				case JsonType.Array:
					return GetHashCode((JsonArray)value);
				case JsonType.Default:
					return 0;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>Compares two <see cref="JsonArray"/> instances returning wether one is equal to another.</summary>
		public bool Equals(JsonArray arrayA, JsonArray arrayB)
		{
			if (arrayA.Count != arrayB.Count)
				return false;

			var enumeratorA = arrayA.GetEnumerator();
			var enumeratorB = arrayB.GetEnumerator();

			while (enumeratorA.MoveNext())
			{
				enumeratorB.MoveNext();
				if (!Equals(enumeratorA.Current, enumeratorB.Current))
					return false; // returning false on first unequal array item
			}
			return true;
		}

		/// <summary>Returns a hash code for the specified <see cref="JsonArray"/> instance.</summary>
		public int GetHashCode(JsonArray array)
		{
			var hashCode = Int32.MinValue;
			foreach (var item in array)
				hashCode ^= GetHashCode(item);
			return hashCode;
		}
	}
}