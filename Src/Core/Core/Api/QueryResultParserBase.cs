using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using CouchDude.Utils;

namespace CouchDude.Api
{
	/// <summary>Query result parser base class.</summary>
	internal abstract class QueryResultParserBase
	{
		protected static IEnumerable<JsonObject> GetRawRows(JsonObject response)
		{
			var rowsArray = response.TryGetValue("rows") as JsonArray;
			if (rowsArray == null)
				throw new ParseException("Query result is expected to contain array 'rows' property");

			return rowsArray.Cast<JsonObject>();
		}

		protected static int? GetOffset(JsonObject response) { return GetIntProperty(response, "offset"); }

		protected static int? GetTotalRows(JsonObject response) { return GetIntProperty(response, "total_rows"); }

		private static int? GetIntProperty(JsonObject jsonObject, string propertyName)
		{
			if (!jsonObject.ContainsKey(propertyName))
				return null;
			var value = jsonObject[propertyName] as JsonPrimitive;
			if (value != null)
			{
				var intValue = value.Value as int?;
				if (intValue != null) 
					return value.Value as int?;
			}

			throw new ParseException("Response property {0} should be integer value", propertyName);
		}

		protected static JsonObject ParseRawResponse(TextReader textReader)
		{
			JsonObject response;
			try
			{
				response = JsonValue.Load(textReader) as JsonObject;
				if(response == null)
					throw new ParseException("Query result is expected to be JSON object");
			}
			catch (Exception e)
			{
				throw new ParseException(e, e.Message);
			}
			return response;
		}
	}
}