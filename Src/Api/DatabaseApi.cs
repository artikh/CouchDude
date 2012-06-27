#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal partial class DatabaseApi : IDatabaseApi
	{
		private readonly CouchApi parent;
		private readonly DbUriConstructor uriConstructor;

		/// <constructor />
		public DatabaseApi(CouchApi parent, DbUriConstructor uriConstructor)
		{
			if (parent == null) throw new ArgumentNullException("parent");

			this.parent = parent;
			this.uriConstructor = uriConstructor;
			Synchronously = new SynchronousDatabaseApi(this);
		}

		public Task Create(bool throwIfExists = true)
		{
			using (SyncContext.SwitchToDefault())
				return parent
					.RequestCouchDb(new HttpRequestMessage(HttpMethod.Put, uriConstructor.DatabaseUri))
					.ContinueWith(
						rt => {
							var response = rt.Result;
							if (response.IsSuccessStatusCode) return;
							var couchError = new CouchError(parent.Settings.Serializer, response);
							if (couchError.IsAlreadyDatabaseExists)
								if (throwIfExists)
									throw new CouchCommunicationException("Database {0} already exists", uriConstructor.DatabaseName);
								else
									return;
							couchError.ThrowCouchCommunicationException();
						});
		}

		/// <summary>Updates database security descriptor.</summary>
		public Task UpdateSecurityDescriptor(DatabaseSecurityDescriptor securityDescriptor)
		{
			var serializer = parent.Settings.Serializer;
			var request = new HttpRequestMessage(HttpMethod.Put, uriConstructor.SecurityDescriptorUri) {
				Content = new JsonContent(serializer.ConvertToJson(securityDescriptor, throwOnError: true))
			};

			using (SyncContext.SwitchToDefault())
				return parent.RequestCouchDb(request).ContinueWith(
					rt => {
						var response = rt.Result;
						if (response.IsSuccessStatusCode) return;
						var couchError = new CouchError(serializer, response);
						couchError.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
						couchError.ThrowCouchCommunicationException();
					});
		}

		public Task Delete()
		{
			using (SyncContext.SwitchToDefault())
				return parent
					.RequestCouchDb(new HttpRequestMessage(HttpMethod.Delete, uriConstructor.DatabaseUri))
					.ContinueWith(
						rt => {
							var response = rt.Result;
							if (!response.IsSuccessStatusCode)
							{
								var error = new CouchError(parent.Settings.Serializer, response);
								error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
								error.ThrowCouchCommunicationException();
							}
						});
		}

		public Task<Attachment> RequestAttachment(string attachmentId, string documentId, string documentRevision = null)
		{
			if (attachmentId.HasNoValue()) throw new ArgumentNullException("attachmentId");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			var attachmentUri = uriConstructor.GetFullAttachmentUri(attachmentId, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, attachmentUri);
			requestMessage.Headers.Accept.Clear();

			using (SyncContext.SwitchToDefault())
				return parent
					.RequestCouchDb(requestMessage)
					.ContinueWith<Attachment>(
						rt => {
							var response = rt.Result;
							if (!response.IsSuccessStatusCode)
							{
								var error = new CouchError(parent.Settings.Serializer, response);
								error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
								if (error.IsAttachmentMissingFromDocument)
									return null;
								error.ThrowDocumentNotFoundIfNedded(documentId, documentRevision);
								error.ThrowStaleStateExceptionIfNedded(
									string.Format("request attachment ID '{0}'", attachmentId), documentId, documentRevision);
								error.ThrowCouchCommunicationException();
							}
							return new HttpResponseMessageAttachment(attachmentId, response);
						});
		}

		public Task<DocumentInfo> SaveAttachment(Attachment attachment, string documentId, string documentRevision = null)
		{
			if (attachment == null) throw new ArgumentNullException("attachment");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			using (SyncContext.SwitchToDefault())
				return SaveAttachmentInternal(attachment, documentId, documentRevision);
		}

		async Task<DocumentInfo> SaveAttachmentInternal(Attachment attachment, string documentId, string documentRevision)
		{
			var attachmentUri = uriConstructor.GetFullAttachmentUri(attachment.Id, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Put, attachmentUri);
			HttpResponseMessage response;
			using (var requestContentStream = await attachment.OpenRead())
			{
				requestMessage.Content = new StreamContent(requestContentStream);
				requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
				response = await parent.RequestCouchDb(requestMessage);
			}
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Settings.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
				error.ThrowStaleStateExceptionIfNedded(
					string.Format("saving attachment ID '{0}'", attachment.Id), documentId, documentRevision);
				error.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response);
		}

		public Task<DocumentInfo> DeleteAttachment(string attachmentId, string documentId, string documentRevision)
		{
			if (attachmentId.HasNoValue()) throw new ArgumentNullException("attachmentId");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");
	
			using (SyncContext.SwitchToDefault())
				return DeleteAttachmentInternal(attachmentId, documentId, documentRevision);
		}

		async Task<DocumentInfo> DeleteAttachmentInternal(string attachmentId, string documentId, string documentRevision)
		{
			var attachmentUri = uriConstructor.GetFullAttachmentUri(attachmentId, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Delete, attachmentUri);
			var response = await parent.RequestCouchDb(requestMessage);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Settings.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
				error.ThrowAttachmentMissingException(attachmentId, documentId, documentRevision);
				error.ThrowDocumentNotFoundIfNedded(documentId, documentRevision);
				error.ThrowStaleStateExceptionIfNedded(
					string.Format("deleting attachment ID '{0}'", attachmentId), documentId, documentRevision);
				error.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response);
		}

		public Task<DatabaseInfo> RequestInfo() 
		{
			using (SyncContext.SwitchToDefault())
				return RequestInfoInternal();
		}

		async Task<DatabaseInfo> RequestInfoInternal()
		{
			var response = await parent.RequestCouchDb(new HttpRequestMessage(HttpMethod.Get, uriConstructor.DatabaseUri));
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Settings.Serializer, response);
				if (!error.IsDatabaseMissing)
					error.ThrowCouchCommunicationException();
				return new DatabaseInfo(false, uriConstructor.DatabaseName);
			}

			var infoJson = await response.Content.ReadAsJsonObjectAsync();
			return new DatabaseInfo(true, uriConstructor.DatabaseName, infoJson);
		}

		public Task<Document> RequestDocument(
			string documentId, string revision, AdditionalDocumentProperty additionalProperties = default(AdditionalDocumentProperty))
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			using (SyncContext.SwitchToDefault())
				return DocumentRequestTask.Start(uriConstructor, this, parent, documentId, revision, additionalProperties);
		}

		public Task<DocumentInfo> SaveDocument(Document document, bool overwriteConcurrentUpdates)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue())
				throw new ArgumentException("Document ID should not be empty or null.", "document");

			using (SyncContext.SwitchToDefault())
				return SaveDocumentTask.Start(this, document, overwriteConcurrentUpdates);
		}

		public Task<DocumentInfo> DeleteDocument(string documentId, string revision)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");

			var documentUri = uriConstructor.GetFullDocumentUri(documentId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			using (SyncContext.SwitchToDefault())
				return DeleteDocumentInternal(documentId, revision, request);
		}

		async Task<DocumentInfo> DeleteDocumentInternal(string documentId, string revision, HttpRequestMessage request)
		{
			var response = await parent.RequestCouchDb(request);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Settings.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				error.ThrowStaleStateExceptionIfNedded("delete", documentId, revision);
				error.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response);
		}

		public Task<DocumentInfo> SaveDocument(Document document) { return SaveDocument(document, overwriteConcurrentUpdates: false); }

		public Task<DocumentInfo> CopyDocument(
			string originalDocumentId, 
			string originalDocumentRevision,
			string targetDocumentId,
			string targetDocumentRevision = null)
		{
			if (string.IsNullOrEmpty(originalDocumentId))
				throw new ArgumentNullException("originalDocumentId");
			if (string.IsNullOrEmpty(targetDocumentId))
				throw new ArgumentNullException("targetDocumentId");

			using (SyncContext.SwitchToDefault())
				return CopyDocumentInternal(originalDocumentId, originalDocumentRevision, targetDocumentId, targetDocumentRevision);
		}

		async Task<DocumentInfo> CopyDocumentInternal(
			string originalDocumentId, string originalDocumentRevision, string targetDocumentId,
			string targetDocumentRevision)
		{
			var fullOriginalDocumentUri = uriConstructor.GetFullDocumentUri(
				originalDocumentId, originalDocumentRevision);
			var request = new HttpRequestMessage(CopyHttpMethod, fullOriginalDocumentUri);
			var targetDocumentUriString = uriConstructor.GetDocumentUriString(
				targetDocumentId, targetDocumentRevision);
			request.Headers.TryAddWithoutValidation("Destination", targetDocumentUriString);

			var response = await parent.RequestCouchDb(request);

			if (!response.IsSuccessStatusCode)
			{
				var couchApiError = new CouchError(parent.Settings.Serializer, response);
				couchApiError.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				couchApiError.ThrowStaleStateExceptionForDocumentCopyIfNedded(
					originalDocumentId, originalDocumentRevision, targetDocumentId, targetDocumentRevision);
				couchApiError.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response);
		}
		
		private static readonly HttpMethod CopyHttpMethod = new HttpMethod("COPY");

		public Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			if (updateCommandBuilder == null) throw new ArgumentNullException("updateCommandBuilder");

			using (SyncContext.SwitchToDefault())
				return BulkUpdateInternal(updateCommandBuilder);
		}

		private async Task<IDictionary<string, DocumentInfo>> BulkUpdateInternal(Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			var unitOfWork = new BulkUpdateBatch(uriConstructor, parent.Settings.Serializer);
			updateCommandBuilder(unitOfWork);
			return unitOfWork.IsEmpty
				? new Dictionary<string, DocumentInfo>(0)
				: await unitOfWork.Execute(request => parent.RequestCouchDb(request));
		}

		public Task<string> RequestLastestDocumentRevision(string documentId)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");

			var documentUri = uriConstructor.GetFullDocumentUri(documentId);
			var request = new HttpRequestMessage(HttpMethod.Head, documentUri);

			using (SyncContext.SwitchToDefault())
				return parent.RequestCouchDb(request).ContinueWith(
					rt => {
						var response = rt.Result;
						if (!response.IsSuccessStatusCode)
						{
							var couchApiError = new CouchError(parent.Settings.Serializer, response);
							couchApiError.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
							couchApiError.ThrowStaleStateExceptionIfNedded("update", documentId);
							couchApiError.ThrowInvalidDocumentExceptionIfNedded(documentId);
							if (response.StatusCode == HttpStatusCode.NotFound)
								return null;
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
			if (query.Skip >= 10)
				throw new ArgumentException("Query skip should be less then 10. http://bit.ly/d9iUeF", "query");

			using (SyncContext.SwitchToDefault())
				return QueryInternal(query);
		}

		 async Task<IViewQueryResult> QueryInternal(ViewQuery query)
		{
			var response = await StartQuery(uriConstructor.GetQueryUri(query));
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Settings.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				error.ThrowViewNotFoundExceptionIfNedded(query);
				error.ThrowCouchCommunicationException();
			}
			using (var reader = await response.Content.ReadAsUtf8TextReaderAsync())
				return ViewQueryResultParser.Parse(reader, query);
		}

		public Task<ILuceneQueryResult> QueryLucene(LuceneQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");

			using (SyncContext.SwitchToDefault())
				return QueryLuceneInternal(query);
		}

		async Task<ILuceneQueryResult> QueryLuceneInternal(LuceneQuery query)
		{
			var response = await StartQuery(uriConstructor.GetQueryUri(query));
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Settings.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				error.ThrowLuceneIndexNotFoundExceptionIfNedded(query);
				error.ThrowCouchCommunicationException();
			}
			using (var reader = await response.Content.ReadAsUtf8TextReaderAsync())
				return LuceneQueryResultParser.Parse(reader, query);
		}

		public ISynchronousDatabaseApi Synchronously { get; private set; }

		private Task<HttpResponseMessage> StartQuery(Uri queryUri)
		{
			return parent.RequestCouchDb(new HttpRequestMessage(HttpMethod.Get, queryUri));
		}

		private static async Task<DocumentInfo> ReadDocumentInfo(HttpResponseMessage response)
		{
			dynamic couchResponse = await response.Content.ReadAsJsonObjectAsync();
			var id = (string) couchResponse.id;
			var revision = (string) couchResponse.rev;

			return new DocumentInfo(id, revision);
		}
	}
}