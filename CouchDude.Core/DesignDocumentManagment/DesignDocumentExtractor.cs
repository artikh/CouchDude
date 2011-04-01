using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.DesignDocumentManagment
{
	/// <summary>Extracts design documents from standard CouchDB list.</summary>
	public class DesignDocumentExtractor: IDesignDocumentExtractor
	{
		/// <summary>Extracts design documents.</summary>
		public IDictionary<string, DesignDocument> Extract(TextReader textReader)
		{			
			if(textReader == null) throw new ArgumentNullException("textReader");
			Contract.Ensures(Contract.Result<IEnumerable<JObject>>() != null);
			Contract.EndContractBlock();

			return ExtractInternal(textReader).ToDictionary(doc => doc.Id, doc => doc);
		}

		private static IEnumerable<DesignDocument> ExtractInternal(TextReader textReader)
		{
			var documentList = ReadDocumentList(textReader);
			return GetRowsArray(documentList).Children<JObject>().Select(GetDocument);
		}

		private static DesignDocument GetDocument(JObject rowObject) 
		{
			var keyProperty = rowObject["key"] as JValue;
			if (keyProperty == null)
				throw new CouchResponseParseException(
					"Document list row object should contain 'key' property.");

			var id = keyProperty.Value<string>();
			if (!id.StartsWith(DesignDocument.IdPrefix))
				throw new CouchResponseParseException(
					"Document list row object's 'key' property should start with " + DesignDocument.IdPrefix + "'.");
			
			var valueProperty = rowObject["value"] as JObject;
			if (valueProperty == null)
				throw new CouchResponseParseException(
					"Document list row object should contain 'value' property.");

			var revProperty = valueProperty["rev"] as JValue;
			if (revProperty == null)
				throw new CouchResponseParseException(
					"Document list row's value property object should contain 'rev' property.");

			var documentProperty = rowObject["doc"] as JObject;
			if (documentProperty == null)
				throw new CouchResponseParseException(
					"Document list row object should contain 'doc' property.");

			return new DesignDocument(documentProperty, id, revProperty.Value<string>());
		}

		private static JArray GetRowsArray(JObject documentList) 
		{
			var rowsArray = documentList["rows"] as JArray;
			if (rowsArray == null)
				throw new CouchResponseParseException(
					"Document list object should contain 'rows' property.");
			return rowsArray;
		}

		private static JObject ReadDocumentList(TextReader textReader) 
		{
			JObject documentList;
			using (var reader = new JsonTextReader(textReader))
				try
				{
					documentList = JObject.Load(reader);
				}
				catch (Exception e)
				{
					throw new CouchResponseParseException(e, "Error parsing document list object");
				}
			return documentList;
		}
	}
}
