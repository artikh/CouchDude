using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Text;

using CouchDude.Core.HttpClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpRequest = CouchDude.Core.HttpClient.HttpRequest;
using HttpResponse = CouchDude.Core.HttpClient.HttpResponse;

namespace CouchDude.Core.Implementation
{
	internal class CouchApi: ICouchApi
	{
		private readonly IHttpClient httpClient;
		private readonly Uri databaseUri;

		/// <constructor />
		public CouchApi(IHttpClient httpClient, Uri serverUri, string databaseName)
		{
			if (httpClient == null) throw new ArgumentNullException("httpClient");
			if (serverUri == null) throw new ArgumentNullException("serverUri");
			if (databaseName == null) throw new ArgumentNullException("databaseName");
			Contract.EndContractBlock();

			this.httpClient = httpClient;
			databaseUri = new Uri(serverUri, databaseName + "/");
		}

		public JObject GetDocumentFromDbById(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequest(documentUri, HttpMethod.Get);
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJObject(response.Body);
		}

		public JObject DeleteDocument(string docId, string revision)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId, revision);
			var request = new HttpRequest(documentUri, HttpMethod.Delete);
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJObject(response.Body);
		}

		public JObject SaveDocumentToDb(string docId, JObject document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequest(
				documentUri, 
				HttpMethod.Put, 
				body: new StringReader(document.ToString(Formatting.None))
			);
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJObject(response.Body);
		}

		public JObject UpdateDocumentInDb(string docId, JObject document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequest(
				documentUri,
				HttpMethod.Put,
				body: new StringReader(document.ToString(Formatting.None))
			);
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJObject(response.Body);
		}

		public string GetLastestDocumentRevision(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequest(documentUri, HttpMethod.Head);
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			var etag = response.Headers[HttpResponseHeader.ETag];
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

		private string GetDocumentUri(string docId, string revision = null)
		{
			const string designDocumentPrefix = "_design/";

			var uriStringBuilder = new StringBuilder(databaseUri.ToString());
			if (docId.StartsWith(designDocumentPrefix))
				uriStringBuilder
					.Append(designDocumentPrefix)
					.Append(docId.Substring(designDocumentPrefix.Length).Replace("/", "%2F"));
			else
				uriStringBuilder.Append(docId.Replace("/", "%2F"));

			if (!string.IsNullOrEmpty(revision))
				uriStringBuilder.Append("?rev=").Append(revision);

			return uriStringBuilder.ToString();
		}

		internal static string ParseErrorResponseBody(TextReader errorTextReader)
		{
			if (errorTextReader == null)
				return null;

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

		private static void ThrowIfNotOk(HttpResponse response)
		{
			if (response.IsOk) return;
			
			if (response.Status == HttpStatusCode.Conflict)
				throw new StaleObjectStateException("Document update conflict detected");
			else
				throw new CouchCommunicationException(
					(ParseErrorResponseBody(response.Body) ?? "Error returned from CouchDB ")
					+ (int) response.Status);
		}

		private HttpResponse MakeRequest(HttpRequest request)
		{
			try
			{
				return httpClient.MakeRequest(request);
			}
			catch (WebException e)
			{
				throw new CouchCommunicationException(e, e.Message);
			}
		}
	}
}