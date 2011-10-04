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
using System.Threading.Tasks;
using CouchDude.Utils;
using CouchDude.Http;

namespace CouchDude.Api
{
	internal class DatabaseApi : IDatabaseApi
	{
		private readonly IHttpClient httpClient;
		private readonly DbUriConstructor uriConstructor;

		/// <constructor />
		public DatabaseApi(IHttpClient httpClient, DbUriConstructor uriConstructor)
		{
			if (httpClient == null) throw new ArgumentNullException("httpClient");

			this.httpClient = httpClient;
			this.uriConstructor = uriConstructor;
			Synchronously = new SynchronousDatabaseApi(this);
		}

		public Task Create()
		{
			return CouchApi.StartRequest(new HttpRequestMessage(HttpMethod.Put, uriConstructor.DatabaseUri), httpClient)
				.ContinueWith(
					t => {
						var response = t.Result;
						if (!response.IsSuccessStatusCode)
							new CouchError(response).ThrowCouchCommunicationException();
					});
		}

		public Task Delete()
		{
			return CouchApi.StartRequest(new HttpRequestMessage(HttpMethod.Delete, uriConstructor.DatabaseUri), httpClient)
				.ContinueWith(
					t =>
					{
						var response = t.Result;
						if (!response.IsSuccessStatusCode)
						{
							var error = new CouchError(response);
							error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
							error.ThrowCouchCommunicationException();
						}
					});
		}

		public Task<IDocumentAttachment> RequestAttachment(string attachmentId, string documentId, string documentRevision = null)
		{
			if (attachmentId.HasNoValue()) throw new ArgumentNullException("attachmentId");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			var attachmentUri = uriConstructor.GetFullAttachmentUri(attachmentId, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, attachmentUri);
			return CouchApi.StartRequest(requestMessage, httpClient)
				.ContinueWith<IDocumentAttachment>(
					rt => {
						var response = rt.Result;
						if (!response.IsSuccessStatusCode)
						{
							var error = new CouchError(response);
							error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
							if (error.IsAttachmentMissingFromDocument)
								return null;
							error.ThrowDocumentNotFoundIfNedded(documentId, documentRevision);
							error.ThrowCouchCommunicationException();
						}
						return new HttpResponseMessageDocumentAttachment(attachmentId, response);
					},
					TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		public Task<DocumentInfo> SaveAttachment(IDocumentAttachment attachment, string documentId, string documentRevision = null)
		{
			if (attachment == null) throw new ArgumentNullException("attachment");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			var attachmentUri = uriConstructor.GetFullAttachmentUri(attachment.Id, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Put, attachmentUri);
			return attachment
				.OpenRead()
				.ContinueWith(
					ort => {
						requestMessage.Content = new StreamContent(ort.Result, attachment.Length);
						return CouchApi.StartRequest(requestMessage, httpClient);
					}, TaskContinuationOptions.OnlyOnRanToCompletion)
				.Unwrap()
				.ContinueWith(
					rt =>
					{
						var response = rt.Result;
						if (!response.IsSuccessStatusCode)
						{
							var error = new CouchError(response);
							error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
							error.ThrowCouchCommunicationException();
						}
						return ReadDocumentInfo(response);
					},
					TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		public Task<DocumentInfo> DeleteAttachment(string attachmentId, string documentId, string documentRevision = null)
		{
			if (attachmentId.HasNoValue()) throw new ArgumentNullException("attachmentId");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			var attachmentUri = uriConstructor.GetFullAttachmentUri(attachmentId, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Delete, attachmentUri);
			return CouchApi.StartRequest(requestMessage, httpClient)
				.ContinueWith<DocumentInfo>(
					rt =>
					{
						var response = rt.Result;
						if (!response.IsSuccessStatusCode)
						{
							var error = new CouchError(response);
							error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
							error.ThrowAttachmentMissingException(attachmentId, documentId, documentRevision);
							error.ThrowDocumentNotFoundIfNedded(documentId, documentRevision);
							error.ThrowCouchCommunicationException();
						}
						return ReadDocumentInfo(response);
					},
					TaskContinuationOptions.OnlyOnRanToCompletion);
			
		}

		public Task<DatabaseInfo> RequestInfo()
		{
			return CouchApi.StartRequest(new HttpRequestMessage(HttpMethod.Get, uriConstructor.DatabaseUri), httpClient)
				.ContinueWith(
					t => {
						var response = t.Result;

						var exists = true;
						IJsonFragment responseJson = null;
						if (!response.IsSuccessStatusCode)
						{
							var error = new CouchError(response);
							if (error.IsDatabaseMissing)
								exists = false;
							else
								error.ThrowCouchCommunicationException();
						}
						else
						{
							using (var reader = response.Content.GetTextReader())
								responseJson = new JsonFragment(reader);
						}
						return new DatabaseInfo(exists, uriConstructor.DatabaseName, responseJson);
					});
		}

		public Task<IDocument> RequestDocument(string documentId, string revision)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			

			var documentUri = uriConstructor.GetFullDocumentUri(documentId, revision);
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);
			return CouchApi.StartRequest(request, httpClient).ContinueWith<IDocument>(
				rt => {
					var response = rt.Result;
					if (!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
						if (response.StatusCode == HttpStatusCode.NotFound)
							return null;
						error.ThrowCouchCommunicationException();
					}
					return new Document(response.GetContentTextReader());
				});
		}

		public Task<DocumentInfo> DeleteDocument(string documentId, string revision)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");
			

			var documentUri = uriConstructor.GetFullDocumentUri(documentId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			return CouchApi.StartRequest(request, httpClient).ContinueWith(
				rt => {
					var response = rt.Result;
					if (!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
						error.ThrowStaleStateExceptionIfNedded("delete", documentId, revision);
						error.ThrowCouchCommunicationException();
					}
					return ReadDocumentInfo(response);
				});
		}

		public Task<DocumentInfo> SaveDocument(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue()) 
				throw new ArgumentException("Document ID should not be empty or noll.", "document");
			

			var documentUri = uriConstructor.GetFullDocumentUri(document.Id);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri) { Content = new JsonContent(document) };
			return CouchApi.StartRequest(request, httpClient).ContinueWith(
				rt => {
					var response = rt.Result;
					var documentId = document.Id;
					if(!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
						error.ThrowStaleStateExceptionIfNedded("update", documentId);
						error.ThrowInvalidDocumentExceptionIfNedded(documentId);
						error.ThrowCouchCommunicationException();
					}
					return ReadDocumentInfo(response);
				});
		}

		public Task<DocumentInfo> CopyDocument(
			string originalDocumentId, 
			string targetDocumentId, 
			string originalDocumentRevision = null, 
			string targetDocumentRevision = null)
		{
			if (string.IsNullOrEmpty(originalDocumentId)) 
				throw new ArgumentNullException("originalDocumentId");
			if (string.IsNullOrEmpty(targetDocumentId))
				throw new ArgumentNullException("targetDocumentId");

			var fullOriginalDocumentUri = uriConstructor.GetFullDocumentUri(originalDocumentId, originalDocumentRevision);
			var request = new HttpRequestMessage(CopyHttpMethod, fullOriginalDocumentUri);
			var targetDocumentUriString = uriConstructor.GetDocumentUriString(targetDocumentId, targetDocumentRevision);
			request.Headers.AddWithoutValidation("Destination", targetDocumentUriString);

			return CouchApi.StartRequest(request, httpClient).ContinueWith(
				rt =>
				{
					var response = rt.Result;

					if (!response.IsSuccessStatusCode)
					{
						var couchApiError = new CouchError(response);
						couchApiError.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
						if (couchApiError.StatusCode == HttpStatusCode.Conflict)
							throw new StaleObjectStateException(
								"Document {0}(rev:{1}) to {2}(rev:{3}) copy conflict detected", 
								originalDocumentId, 
								originalDocumentRevision, 
								targetDocumentId, 
								targetDocumentRevision
							);
						couchApiError.ThrowCouchCommunicationException();
					}
					return ReadDocumentInfo(response);
				});
		}

		private static readonly IDictionary<string, DocumentInfo> EmptyDictionary = new Dictionary<string, DocumentInfo>(0);
		private static readonly HttpMethod CopyHttpMethod = new HttpMethod("COPY");

		public Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			if (updateCommandBuilder == null) throw new ArgumentNullException("updateCommandBuilder");
			
			var unitOfWork = new BulkUpdateBatch(uriConstructor);
			updateCommandBuilder(unitOfWork);
			return unitOfWork.IsEmpty 
				? Task.Factory.StartNew(() => EmptyDictionary) 
				: unitOfWork.Execute(httpClient, request => CouchApi.StartRequest(request, httpClient));
		}

		public Task<string> RequestLastestDocumentRevision(string documentId)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			
			var documentUri = uriConstructor.GetFullDocumentUri(documentId);
			var request = new HttpRequestMessage(HttpMethod.Head, documentUri);
			return CouchApi.StartRequest(request, httpClient).ContinueWith(
				rt => {
					var response = rt.Result;
					
					if (!response.IsSuccessStatusCode)
					{
						var couchApiError = new CouchError(response);
						couchApiError.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
						couchApiError.ThrowStaleStateExceptionIfNedded("update", documentId);
						couchApiError.ThrowInvalidDocumentExceptionIfNedded(documentId);
						if (response.StatusCode == HttpStatusCode.NotFound)
							return (string)null;
						couchApiError.ThrowCouchCommunicationException();
					}

					var etag = response.Headers.ETag;
					if (etag == null || etag.Tag == null)
						throw new ParseException("Etag header expected but was not found.");
					return etag.Tag.Trim('"');
				});
		}

		public Task<IViewQueryResult> Query(ViewQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (query.Skip >= 10) throw new ArgumentException("Query skip should be less then 10. http://bit.ly/d9iUeF", "query");

			return StartQuery(query).ContinueWith(
				rt => {
					var response = rt.Result;
					if(!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
						error.ThrowViewNotFoundExceptionIfNedded(query);
						error.ThrowCouchCommunicationException();
					}
					return ViewQueryResultParser.Parse(rt.Result.GetContentTextReader(), query);
				});
		}

		public Task<ILuceneQueryResult> QueryLucene(LuceneQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			
			return StartQuery(query).ContinueWith(
				rt => {
					var response = rt.Result;
					if (!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
						error.ThrowLuceneIndexNotFoundExceptionIfNedded(query);
						error.ThrowCouchCommunicationException();
					}
					return LuceneQueryResultParser.Parse(rt.Result.GetContentTextReader(), query);
				});
		}

		public ISynchronousDatabaseApi Synchronously { get; private set; }

		private Task<HttpResponseMessage> StartQuery(IQuery query)
		{
			var viewUri = uriConstructor.GetQueryUri(query);
			var request = new HttpRequestMessage(HttpMethod.Get, viewUri);
			return CouchApi.StartRequest(request, httpClient);
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