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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CouchDude.Utils;
using CouchDude.Http;

namespace CouchDude.Api
{
	internal class CouchApi : ICouchApi
	{
		private readonly IHttpClient httpClient;
		private readonly Uri databaseUri;

		/// <constructor />
		public CouchApi(IHttpClient httpClient, Uri serverUri, string databaseName)
		{
			if (httpClient == null) throw new ArgumentNullException("httpClient");
			if (serverUri == null) throw new ArgumentNullException("serverUri");
			if (databaseName.HasNoValue()) throw new ArgumentNullException("databaseName");
			

			this.httpClient = httpClient;
			databaseUri = new Uri(serverUri, databaseName + "/");
			Synchronously = new SynchronousCouchApi(this);
		}

		public Task<IDocument> RequestDocumentById(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);
			return StartRequest(request).ContinueWith<IDocument>(
				rt => {
					var response = rt.Result;
					if (response.StatusCode == HttpStatusCode.NotFound)
						return null;
					if (!response.IsSuccessStatusCode)
							throw Errors.CreateCommunicationException(response);
					return new Document(response.GetContentTextReader());
				});
		}

		public Task<DocumentInfo> DeleteDocument(string documentId, string revision)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");
			

			var documentUri = GetDocumentUri(documentId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			return StartRequest(request).ContinueWith(
				rt => {
					var response = rt.Result;
					if (!response.IsSuccessStatusCode)
						if(response.StatusCode == HttpStatusCode.Conflict)
							throw Errors.CreateStaleStateException("delete", documentId, revision);
						else 
							throw Errors.CreateCommunicationException(response);
					return ReadDocumentInfo(response);
				});
		}

		public Task<DocumentInfo> SaveDocument(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue()) 
				throw new ArgumentException("Document ID should not be empty or noll.", "document");
			

			var documentUri = GetDocumentUri(document.Id);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri) { Content = new JsonContent(document) };
			return StartRequest(request).ContinueWith(
				rt => {
					var response = rt.Result;
					var documentId = document.Id;
					if(!response.IsSuccessStatusCode)
						switch(response.StatusCode)
						{
							case HttpStatusCode.Conflict:
								throw Errors.CreateStaleStateException("update", documentId);
							case HttpStatusCode.Forbidden:
								throw Errors.CreateInvalidDocumentException(documentId, response);
							default:
								throw Errors.CreateCommunicationException(response);
						}
					return ReadDocumentInfo(response);
				});
		}

		public Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateUnitOfWork> updateCommandBuilder)
		{
			if (updateCommandBuilder == null) throw new ArgumentNullException("updateCommandBuilder");
			var unitOfWork = new BulkUpdateUnitOfWork();
			updateCommandBuilder(unitOfWork);
			if(unitOfWork.IsEmpty)
				throw new ArgumentException("Builder should invoke at least one method on unit of work", "updateCommandBuilder");

			return unitOfWork.Execute(httpClient, StartRequest, databaseUri);
		}
		
		public Task<string> RequestLastestDocumentRevision(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			
			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Head, documentUri);
			return StartRequest(request).ContinueWith(
				rt => {
					var response = rt.Result;

					if (response.StatusCode == HttpStatusCode.NotFound)
						return null;

					if(!response.IsSuccessStatusCode)
						switch(response.StatusCode)
						{
							case HttpStatusCode.Conflict:
								throw Errors.CreateStaleStateException("update", docId);
							case HttpStatusCode.Forbidden:
								throw Errors.CreateInvalidDocumentException(docId, Errors.ParseErrorResponseBody(response));
							default:
								throw Errors.CreateCommunicationException(response);
						}
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
			

			return StartQuery(query.ToUri()).ContinueWith<IPagedList<ViewResultRow>>(
				rt => {
					var response = rt.Result;
					Errors.ThrowIfViewRequestWasUnsuccessful(query, response);
					return ViewResultParser.Parse(rt.Result.GetContentTextReader(), query);
				});
		}

		public Task<IPagedList<LuceneResultRow>> QueryLucene(LuceneQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			
			return StartQuery(query.ToUri()).ContinueWith<IPagedList<LuceneResultRow>>(
				rt => {
					var response = rt.Result;
					Errors.ThrowIfFulltextIndexRequestWasUnsuccessful(query, response);
					return LuceneResultParser.Parse(rt.Result.GetContentTextReader(), query);
				});
		}

		public ISynchronousCouchApi Synchronously { get; private set; }

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

		private Task<HttpResponseMessage> StartRequest(HttpRequestMessage request)
		{
			return httpClient
				.StartRequest(request)
				.ContinueWith(
					t => {
						if (t.IsFaulted && t.Exception != null)
						{
							var innerExceptions = t.Exception.InnerExceptions;
							

							var newInnerExceptions = new Exception[innerExceptions.Count];
							for (var i = 0; i < innerExceptions.Count; i++)
							{
								var e = innerExceptions[i];
								newInnerExceptions[i] = 
									e is WebException || e is SocketException || e is HttpException
										? new CouchCommunicationException(e)
										: e;
							}
							throw new AggregateException(t.Exception.Message, newInnerExceptions);
						}
						return t.Result;
					});
		}

		private static DocumentInfo ReadDocumentInfo(HttpResponseMessage response)
		{
			dynamic couchResponse = new JsonFragment(response.GetContentTextReader());
			var id = (string)couchResponse.id;
			var revision = (string)couchResponse.rev;

			return new DocumentInfo(id, revision);
		}
	}
}