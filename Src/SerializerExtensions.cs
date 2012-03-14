using System.IO;
using System.Json;
using JetBrains.Annotations;

namespace CouchDude
{
	/// <summary>Extension methods over <see cref="ISerializer"/></summary>
	public static class SerializerExtensions
	{
		/// <summary>Deserializes object of provided type from provided instance of <see cref="TextReader"/>.</summary>
		public static T Deserialize<T>(
			this ISerializer self, [NotNull]TextReader source, bool throwOnError)
		{
			return (T)self.Deserialize(typeof(T), source, throwOnError);
		}

		/// <summary>Converts provided JSON to object of provided type.</summary>
		public static T ConvertFromJson<T>(
			this ISerializer self, [NotNull]JsonValue source, bool throwOnError)
		{
			return (T)self.ConvertFromJson(typeof(T), source, throwOnError);
		}
	}
}