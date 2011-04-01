using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Implementation
{
	/// <summary>Utility method class.</summary>
	public static class Utils
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

		/// <summary>Grabs optional property value from provided object returning
		/// null if there is no such.</summary>
		public static string GetOptionalProperty(this JObject doc, string name)
		{
			var propertyValue = doc[name] as JValue;
			return propertyValue == null ? null : propertyValue.Value<string>();
		}

		/// <summary>Generates unique ID string based on GUID.</summary>
		public static string GenerateId()
		{
			var bytes = Guid.NewGuid().ToByteArray();
			var id = new StringBuilder(Convert.ToBase64String(bytes));
			for (var i = 0; i < id.Length; i++)
				switch (id[i])
				{
					case '+':
						id[i] = '_';
						break;
					case '/':
						id[i] = '-';
						break;
				}
			return id.ToString();
		}
	}
}
