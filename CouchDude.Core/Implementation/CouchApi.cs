using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Implementation
{
	internal class CouchApi: ICouchApi
	{
		private readonly IHttp http;
		private readonly Uri databaseUri;

		/// <constructor />
		public CouchApi(IHttp http, Uri databaseUri)
		{
			this.http = http;
			this.databaseUri = databaseUri;
		}

		public JObject GetDocumentFromDbById(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			TextReader responseTextReader1;
			try
			{
				responseTextReader1 = http.RequestAndOpenTextReader(documentUri, "GET", null);
			}
			catch (WebException e)
			{
				throw new CouchCommunicationException(e, TryGetCouchErrorMessage(e) ?? e.Message);
			}
			var responseTextReader = responseTextReader1;
			var document = ReadJObject(responseTextReader);
			return document;
		}

		public JObject SaveDocumentToDb(string docId, JObject document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			TextReader responseTextReader;
			using (var documentStringReader = new StringReader(document.ToString(Formatting.None)))
				try
				{
					responseTextReader = 
						http.RequestAndOpenTextReader(documentUri, "PUT", documentStringReader);
				}
				catch (WebException e)
				{
					throw new CouchCommunicationException(e, TryGetCouchErrorMessage(e) ?? e.Message);
				}
			var responseJObject = ReadJObject(responseTextReader);
			if (responseJObject == null)
				throw new CouchResponseParseException(
					"CouchDB have not returned response object when saving document.");
			return responseJObject;
		}

		public JObject UpdateDocumentInDb(string docId, JObject document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			TextReader responseTextReader;
			using (var documentStringReader = new StringReader(document.ToString(Formatting.None)))
				try
				{
					responseTextReader = 
						http.RequestAndOpenTextReader(documentUri, "PUT", documentStringReader);
				}
				catch(WebException e)
				{
					var httpWebResponse = (HttpWebResponse)e.Response;
					if (httpWebResponse.StatusCode == HttpStatusCode.Conflict)
						throw new StaleObjectStateException(
							"Document update conflict detected:\n{0}", document);
					else
						throw new CouchCommunicationException(e, TryGetCouchErrorMessage(e) ?? e.Message);
				}
			var responseJObject = ReadJObject(responseTextReader);
			if (responseJObject == null)
				throw new CouchResponseParseException(
					"CouchDB have not returned response object when saving document.");
			return responseJObject;
		}

		public string GetLastestDocumentRevision(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			WebHeaderCollection headers;
			var documentUri = GetDocumentUri(docId);
				try
				{
					headers = http.RequestAndGetHeaders(documentUri, "HEAD");
				}
				catch (WebException e)
				{
					throw new CouchCommunicationException(e, TryGetCouchErrorMessage(e) ?? e.Message);
				}
			var etag = headers[HttpResponseHeader.ETag];
			if (string.IsNullOrEmpty(etag))
				throw new CouchResponseParseException("Etag header expected.");
			return etag;
		}

		private static JObject ReadJObject(TextReader responseTextReader)
		{
			JObject response = null;
			try
			{
				if (responseTextReader != null)
					using (responseTextReader)
					using (var jsonReader = new JsonTextReader(responseTextReader))
						response = JToken.ReadFrom(jsonReader) as JObject;
			}
			catch (Exception e)
			{
				throw new CouchResponseParseException(
					e, "Error reading JSON recived from CouchDB: ", e.Message);
			}

			if (response == null)
				throw new CouchResponseParseException(
					"CouchDB was expected to return JSON object.");

			return response;
		}

		private Uri GetDocumentUri(string docId)
		{
			return new Uri(databaseUri, docId);
		}

		private static T GetErrorJToken<T>(WebException webException)
			where T : JToken
		{
			try
			{
				var response = (HttpWebResponse)webException.Response;
				using (var stream = response.GetResponseStream())
					if (stream != null)
						using (var errorTextReader = new StreamReader(stream, Http.GetEncoding(response)))
						using (var jsonReader = new JsonTextReader(errorTextReader))
							return JToken.ReadFrom(jsonReader) as T;
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static string TryGetCouchErrorMessage(WebException webException)
		{
			try
			{
				var response = (HttpWebResponse)webException.Response;
				using (var stream = response.GetResponseStream())
					if(stream != null)
						using (var textReader = new StreamReader(stream, Http.GetEncoding(response)))
							return ParseErrorResponseBody(textReader);
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}
		
		internal static string ParseErrorResponseBody(TextReader errorTextReader)
		{
			JObject errorObject;
			using (var jsonReader = new JsonTextReader(errorTextReader))
				try
				{
					errorObject = JObject.Load(jsonReader);
				}
				catch (JsonReaderException)
				{
					return null;
				}

			var errorNameToken = errorObject["error"];
			var errorName = errorNameToken == null ? String.Empty : errorNameToken.Value<string>();

			var reasonMessageToken = errorObject["reason"];
			var reasonMessage = reasonMessageToken == null ? String.Empty : reasonMessageToken.Value<string>();

			var message = new StringBuilder();
			message.Append(errorName);

			if (message.Length > 0 && reasonMessage.Length > 0)
				message.Append(": ");
			message.Append(reasonMessage);
			return message.Length > 0 ? message.ToString() : null;
		}
	}
}