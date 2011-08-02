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
using System.Threading.Tasks;
using CouchDude.Core.Utils;
using CouchDude.Core.Http;

namespace CouchDude.Core.Api
{
	internal partial class CouchApi: ICouchApi
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

		public Task<IDocument> RequestDocumentById(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);
			return StartRequest(request).ContinueWith<IDocument>(
				rt => {
					var response = rt.Result;
					if (response.StatusCode == HttpStatusCode.NotFound)
						return null;
					ThrowIfNotOk(response);
					return new Document(response.GetContentTextReader());
				});
		}

		public Task<IJsonFragment> DeleteDocument(string docId, string revision)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			return StartRequest(request).ContinueWith<IJsonFragment>(
				rt => {
					var response = rt.Result;
					ThrowIfNotOk(response);
					return new JsonFragment(response.GetContentTextReader());
				});
		}
		
		public Task<IJsonFragment> SaveDocument(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue()) 
				throw new ArgumentException("Document ID should not be empty or noll.", "document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(document.Id);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri);
			request.SetStringContent(document.ToString());
			return StartRequest(request).ContinueWith<IJsonFragment>(
				rt => {
					var response = rt.Result;
					ThrowIfNotOk(response);
					return new JsonFragment(response.GetContentTextReader());
				});
		}

		public Task<IJsonFragment> UpdateDocument(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue())
				throw new ArgumentException("Document ID should not be empty or noll.", "document");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(document.Id);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri);
			request.SetStringContent(document.ToString());
			return StartRequest(request).ContinueWith<IJsonFragment>(
				rt => {
					var response = rt.Result;
					ThrowIfNotOk(response);
					return new JsonFragment(response.GetContentTextReader());
				});
		}

		public Task<string> RequestLastestDocumentRevision(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			Contract.EndContractBlock();

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Head, documentUri);
			return StartRequest(request).ContinueWith(
				rt =>
				{
					var response = rt.Result;

					if (response.StatusCode == HttpStatusCode.NotFound)
						return null;

					ThrowIfNotOk(response);
					var etag = response.Headers.ETag;
					if (etag == null || etag.Tag == null)
						throw new ParseException("Etag header expected but was not found.");
					return etag.Tag.Trim('"');
				});
		}
		
		public Task<IPagedList<ViewResultRow>> Query(ViewQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (query.Skip >= 10) throw new ArgumentException("Query skip should be less then 10. http://bit.ly/d9iUeF", "query");
			Contract.EndContractBlock();

			return StartQuery(query.ToUri()).ContinueWith<IPagedList<ViewResultRow>>(
				rt => ViewResultParser.Parse(rt.Result.GetContentTextReader(), query)
			);
		}
		
		public Task<IPagedList<LuceneResultRow>> QueryLucene(LuceneQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			Contract.EndContractBlock();

			return StartQuery(query.ToUri()).ContinueWith<IPagedList<LuceneResultRow>>(
				rt => LuceneResultParser.Parse(rt.Result.GetContentTextReader(), query)
			);
		}

		private Task<HttpResponseMessage> StartQuery(Uri uri)
		{
			var viewUri = new Uri(databaseUri, uri);
			var request = new HttpRequestMessage(HttpMethod.Get, viewUri);
			var task = StartRequest(request);
			return task;
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

			dynamic errorObject;
			using (errorTextReader)
				try
				{
					errorObject = new JsonFragment(errorTextReader);
				}
				catch (Exception)
				{
					return null;
				}

			string errorName = errorObject.error ?? string.Empty;
			string reasonMessage = errorObject.reason ?? string.Empty;

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

		private Task<HttpResponseMessage> StartRequest(HttpRequestMessage request)
		{
			try
			{
				return httpClient.StartRequest(request);
			}
			catch (SocketException e)
			{
				throw new CouchCommunicationException(e);
			}
			catch (WebException e)
			{
				throw new CouchCommunicationException(e);
			}
		}
	}
}