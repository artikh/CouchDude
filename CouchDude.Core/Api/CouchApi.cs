#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

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

		public Document GetDocumentFromDbById(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);
			var response = MakeRequest(request);
			if (response.StatusCode == HttpStatusCode.NotFound)
				return null;
			ThrowIfNotOk(response);
			return ReadDocument(response.GetContentTextReader());
		}

		public JsonFragment DeleteDocument(string docId, string revision)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJson(response.GetContentTextReader());
		}

		public JsonFragment SaveDocumentToDb(string docId, Document document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri);
			request.SetStringContent(document.ToString());
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJson(response.GetContentTextReader());
		}

		public JsonFragment UpdateDocumentInDb(string docId, Document document)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (document == null) throw new ArgumentNullException("document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri);
			request.SetStringContent(document.ToString());
			var response = MakeRequest(request);
			ThrowIfNotOk(response);
			return ReadJson(response.GetContentTextReader());
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
                throw new ParseException("Etag header expected but was not found.");
			return etag.Tag.Trim('"');
		}

		/// <inheritdoc/>
		public ViewResult Query(ViewQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (query.Skip >= 10) throw new ArgumentException("Query skip should be less then 10. http://bit.ly/d9iUeF", "query");
			Contract.EndContractBlock();
			/*
			var viewUri = databaseUri + query.ToUri();
			var request = new HttpRequestMessage(HttpMethod.Get, viewUri);
			var response = MakeRequest(request);
			*/
			return ViewResult.Empty;
		}

		/// <inheritdoc/>
		/// TODO: Неплохо бы засовывать вес поиска в результаты
		public LuceneResult FulltextQuery(LuceneQuery query)
		{
			if (query == null) 
				throw new ArgumentNullException("query");
			Contract.EndContractBlock();
			/*
			var viewUri = databaseUri + query.ToUri();
			var request = new HttpRequestMessage(HttpMethod.Get, viewUri);
			var response = MakeRequest(request);
			*/

			return LuceneResult.Empty;
		}

		private static Document ReadDocument(TextReader responseTextReader)
		{
			return new Document(responseTextReader);
		}

		private static JsonFragment ReadJson(TextReader responseTextReader)
		{
			return new JsonFragment(responseTextReader);
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