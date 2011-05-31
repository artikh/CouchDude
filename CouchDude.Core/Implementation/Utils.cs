using System;
using System.Reflection;
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

		// System.UriSyntaxFlags is internal, so let's duplicate the flag privately
		private const int UnEscapeDotsAndSlashes = 0x2000000;
		private const int SimpleUserSyntax = 0x20000;

		/// <summary>Reverts default <see cref="Uri"/> behaviour of unescaping slashes and dots in path.</summary>
		public static Uri LeaveDotsAndSlashesEscaped(this Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			FieldInfo fieldInfo = uri.GetType().GetField("m_Syntax", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Syntax' field not found");

			object uriParser = fieldInfo.GetValue(uri);
			fieldInfo = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Flags' field not found");

			object uriSyntaxFlags = fieldInfo.GetValue(uriParser);

			// Clear the flag that we don't want
			uriSyntaxFlags = (int)uriSyntaxFlags & ~UnEscapeDotsAndSlashes;
			uriSyntaxFlags = (int)uriSyntaxFlags & ~SimpleUserSyntax;
			fieldInfo.SetValue(uriParser, uriSyntaxFlags);

			return uri;
		}
	}
}
