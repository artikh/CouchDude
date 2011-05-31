using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using CouchDude.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CouchDude.Core.Http;
using JsonSerializer = CouchDude.Core.Utils.JsonSerializer;

namespace CouchDude.Core.Api
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
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);
			var response = MakeRequest(request);
			if (response.StatusCode == HttpStatusCode.NotFound)
				return null;
			ThrowIfNotOk(response);
			return ReadJObject(response.GetContentTextReader());
		}

		public JObject DeleteDocument(string docId, string revision)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJObject(response.GetContentTextReader());
		}

		public JObject SaveDocumentToDb(string docId, JObject document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri);
			request.WriteContentText(document.ToString(Formatting.None));
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJObject(response.GetContentTextReader());
		}

		public JObject UpdateDocumentInDb(string docId, JObject document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri);
			request.WriteContentText(document.ToString(Formatting.None));
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJObject(response.GetContentTextReader());
		}

		public string GetLastestDocumentRevision(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Head, documentUri);
			var response = MakeRequest(request);
			if (response.StatusCode == HttpStatusCode.NotFound)
				return null;

			ThrowIfNotOk(response);
			var etag = response.Headers.ETag;
			if (etag == null || etag.Tag == null)
				throw new CouchResponseParseException("Etag header expected but was not found.");
			return etag.Tag.Trim('"');
		}

		/// <inheritdoc/>
		public ViewResult Query(ViewQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (query.Skip >= 10) throw new ArgumentException("Query skip should be less then 10. http://bit.ly/d9iUeF", "query");
			Contract.EndContractBlock();

			var viewUri = databaseUri + query.ToUri();
			var request = new HttpRequestMessage(HttpMethod.Get, viewUri);
			var response = MakeRequest(request);
			ViewResult viewResult;
			using (var responseBodyReader = response.GetContentTextReader())
			using (var jsonReader = new JsonTextReader(responseBodyReader))
				viewResult = JsonSerializer.Instance.Deserialize<ViewResult>(jsonReader);
			viewResult.Query = query;
			return viewResult;
		}

		/// <inheritdoc/>
		/// TODO: Неплохо бы засовывать вес поиска в результаты
		public LuceneResult FulltextQuery(LuceneQuery query)
		{
			if (query == null) 
				throw new ArgumentNullException("query");
			Contract.EndContractBlock();

			var viewUri = databaseUri + query.ToUri();
			var request = new HttpRequestMessage(HttpMethod.Get, viewUri);
			var response = MakeRequest(request);
			LuceneResult viewResult;
			using (var responseBodyReader = response.GetContentTextReader())
			using (var jsonReader = new JsonTextReader(responseBodyReader))
				viewResult = JsonSerializer.Instance.Deserialize<LuceneResult>(jsonReader);
			viewResult.Query = query;
			return viewResult;
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

		private Uri GetDocumentUri(string docId, string revision = null)
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

			return new Uri(uriStringBuilder.ToString()).LeaveDotsAndSlashesEscaped();
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
				catch (Exception)
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

		private static void ThrowIfNotOk(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode) return;

			if (response.StatusCode == HttpStatusCode.Conflict)
				throw new StaleObjectStateException("Document update conflict detected");
			throw new CouchCommunicationException(
				(ParseErrorResponseBody(response.GetContentTextReader()) ?? "Error returned from CouchDB")
				+ " "
				+ (int) response.StatusCode);
		}

		private HttpResponseMessage MakeRequest(HttpRequestMessage request)
		{
			try
			{
				return httpClient.MakeRequest(request);
			}
			catch (SocketException e)
			{
				throw new CouchCommunicationException(e, e.Message);
			}
			catch (WebException e)
			{
				throw new CouchCommunicationException(e, e.Message);
			}
		}
	}
}