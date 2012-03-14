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

		protected static int GetOffset(JsonObject response)
		{
			return response.GetPrimitiveProperty("offset", -1);
		}

		protected static int GetTotalRows(JsonObject response)
		{
			return response.GetPrimitiveProperty("total_rows", -1);
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