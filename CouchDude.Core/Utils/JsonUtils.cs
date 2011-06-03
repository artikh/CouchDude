using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Utils
{
	/// <summary>Utility method class.</summary>
	public static class JsonUtils
	{
		/// <summary>Grabs required property value from provided object throwing 
		/// <see cref="CouchResponseParseException"/> if not found or empty.</summary>
		public static string GetRequiredProperty(this JObject doc, string name, string additionalMessage = null)
		{
			var propertyValue = doc[name] as JValue;
			if (propertyValue == null)
				throw new CouchResponseParseException(
					"Required field '{0}' have not found on document{1}:\n {2}",
					name,
					additionalMessage == null? string.Empty: ". " + additionalMessage,
					doc.ToString(Formatting.None)
				);
			var value = propertyValue.Value<string>();
			if(string.IsNullOrWhiteSpace(value))
				throw new CouchResponseParseException("Required field '{0}' is empty", name);

			return value;
		}
	}
}
