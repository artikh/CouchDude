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
using System.Json;
using System.Net;
using System.Net.Http;
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

		public async Task Create(bool throwIfExists = true)
		{
			var response = await parent
				.RequestCouchDb(new HttpRequestMessage(HttpMethod.Put, uriConstructor.DatabaseUri)).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var couchError = new CouchError(parent.Serializer, response);
				if (couchError.IsAlreadyDatabaseExists)
					if (throwIfExists)
						throw new CouchCommunicationException(
							"Database {0} already exists", uriConstructor.DatabaseName);
					else
						return;
				couchError.ThrowCouchCommunicationException();
			}
		}

		public async Task Delete()
		{
			var response = await parent
				.RequestCouchDb(new HttpRequestMessage(HttpMethod.Delete, uriConstructor.DatabaseUri)).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
				error.ThrowCouchCommunicationException();
			}
		}

		public async Task<DocumentAttachment> RequestAttachment(
			string attachmentId, string documentId, string documentRevision = null)
		{
			if (attachmentId.HasNoValue()) throw new ArgumentNullException("attachmentId");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			var attachmentUri = uriConstructor.GetFullAttachmentUri(attachmentId, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, attachmentUri);

			var response = await parent.RequestCouchDb(requestMessage).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
				if (error.IsAttachmentMissingFromDocument)
					return null;
				error.ThrowDocumentNotFoundIfNedded(documentId, documentRevision);
				error.ThrowStaleStateExceptionIfNedded(
					string.Format("request attachment ID '{0}'", attachmentId), documentId, documentRevision);
				error.ThrowCouchCommunicationException();
			}
			return new HttpResponseMessageDocumentAttachment(attachmentId, response);
		}

		public async Task<DocumentInfo> SaveAttachment(
			DocumentAttachment attachment, string documentId, string documentRevision = null)
		{
			if (attachment == null) throw new ArgumentNullException("attachment");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			var attachmentUri = uriConstructor.GetFullAttachmentUri(
				attachment.Id, documentId, documentRevision);

			HttpResponseMessage response;
			var requestMessage = new HttpRequestMessage(HttpMethod.Put, attachmentUri);
			using (var requestContentStream = await attachment.OpenRead().ConfigureAwait(false))
			{
				requestMessage.Content = new StreamContent(requestContentStream);
				response = await parent.RequestCouchDb(requestMessage).ConfigureAwait(false);
			}
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
				error.ThrowStaleStateExceptionIfNedded(
					string.Format("saveing attachment ID '{0}'", attachment.Id), documentId, documentRevision);
				error.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response).ConfigureAwait(false);
		}

		public async Task<DocumentInfo> DeleteAttachment(
			string attachmentId, string documentId, string documentRevision = null)
		{
			if (attachmentId.HasNoValue()) throw new ArgumentNullException("attachmentId");
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");

			var attachmentUri = uriConstructor.GetFullAttachmentUri(
				attachmentId, documentId, documentRevision);
			var requestMessage = new HttpRequestMessage(HttpMethod.Delete, attachmentUri);
			var response = await parent.RequestCouchDb(requestMessage).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
				error.ThrowAttachmentMissingException(attachmentId, documentId, documentRevision);
				error.ThrowDocumentNotFoundIfNedded(documentId, documentRevision);
				error.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response).ConfigureAwait(false);
		}

		public async Task<DatabaseInfo> RequestInfo()
		{
			var response =
				await parent.RequestCouchDb(new HttpRequestMessage(HttpMethod.Get, uriConstructor.DatabaseUri)).ConfigureAwait(false);
			var exists = true;
			JsonObject responseJson = null;
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				if (error.IsDatabaseMissing)
					exists = false;
				else
					error.ThrowCouchCommunicationException();
			}
			else
			{
				responseJson = await response.Content.ReadAsJsonObjectAsync().ConfigureAwait(false);
			}
			return new DatabaseInfo(exists, uriConstructor.DatabaseName, responseJson);
		}

		public async Task<Document> RequestDocument(string documentId, string revision)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			
			var documentUri = uriConstructor.GetFullDocumentUri(documentId, revision);
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);

			var response = await parent.RequestCouchDb(request).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				if (response.StatusCode == HttpStatusCode.NotFound)
					return null;
				error.ThrowCouchCommunicationException();
			}
			using (var reader = await response.Content.ReadAsTextReaderAsync().ConfigureAwait(false))
				return new Document(reader);
		}

		public Task<DocumentInfo> SaveDocument(Document document, bool overwriteConcurrentUpdates)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue())
				throw new ArgumentException("Document ID should not be empty or null.", "document");

			return DocumentSaver.StartSaving(this, document, overwriteConcurrentUpdates);
		}

		public async Task<DocumentInfo> DeleteDocument(string documentId, string revision)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");


			var documentUri = uriConstructor.GetFullDocumentUri(documentId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			var response = await parent.RequestCouchDb(request).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				error.ThrowStaleStateExceptionIfNedded("delete", documentId, revision);
				error.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response).ConfigureAwait(false);
		}

		public Task<DocumentInfo> SaveDocument(Document document) { return SaveDocument(document, overwriteConcurrentUpdates: false); }

		public async Task<DocumentInfo> CopyDocument(string originalDocumentId, string originalDocumentRevision, string targetDocumentId, string targetDocumentRevision = null)
		{
			if (string.IsNullOrEmpty(originalDocumentId))
				throw new ArgumentNullException("originalDocumentId");
			if (string.IsNullOrEmpty(targetDocumentId))
				throw new ArgumentNullException("targetDocumentId");

			var fullOriginalDocumentUri = uriConstructor.GetFullDocumentUri(originalDocumentId, originalDocumentRevision);
			var request = new HttpRequestMessage(CopyHttpMethod, fullOriginalDocumentUri);
			var targetDocumentUriString = uriConstructor.GetDocumentUriString(targetDocumentId, targetDocumentRevision);
			request.Headers.AddWithoutValidation("Destination", targetDocumentUriString);

			var response = await parent.RequestCouchDb(request).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				var couchApiError = new CouchError(parent.Serializer, response);
				couchApiError.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				couchApiError.ThrowStaleStateExceptionForDocumentCopyIfNedded(
					originalDocumentId, originalDocumentRevision, targetDocumentId, targetDocumentRevision);
				couchApiError.ThrowCouchCommunicationException();
			}
			return await ReadDocumentInfo(response).ConfigureAwait(false);
		}

		private static readonly IDictionary<string, DocumentInfo> EmptyDictionary =
			new Dictionary<string, DocumentInfo>(0);

		private static readonly HttpMethod CopyHttpMethod = new HttpMethod("COPY");

		public Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			if (updateCommandBuilder == null) throw new ArgumentNullException("updateCommandBuilder");

			return BulkUpdateInternal(updateCommandBuilder);
		}

		private async Task<IDictionary<string, DocumentInfo>> BulkUpdateInternal(Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			var unitOfWork = new BulkUpdateBatch(uriConstructor, parent.Serializer);
			updateCommandBuilder(unitOfWork);
			if (unitOfWork.IsEmpty)
				return EmptyDictionary;
			else
				return await unitOfWork.Execute(request => parent.RequestCouchDb(request)).ConfigureAwait(false);
		}

		public async Task<string> RequestLastestDocumentRevision(string documentId)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");

			var documentUri = uriConstructor.GetFullDocumentUri(documentId);
			var request = new HttpRequestMessage(HttpMethod.Head, documentUri);

			var response = await parent.RequestCouchDb(request).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				var couchApiError = new CouchError(parent.Serializer, response);
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
				
		}

		public async Task<IViewQueryResult> Query(ViewQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			if (query.Skip >= 10)
				throw new ArgumentException("Query skip should be less then 10. http://bit.ly/d9iUeF", "query");

			var response = await StartQuery(uriConstructor.GetQueryUri(query)).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				error.ThrowViewNotFoundExceptionIfNedded(query);
				error.ThrowCouchCommunicationException();
			}
			using (var reader = await response.Content.ReadAsTextReaderAsync().ConfigureAwait(false))
				return ViewQueryResultParser.Parse(reader, query);
		}

		public async Task<ILuceneQueryResult> QueryLucene(LuceneQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");

			var response = await StartQuery(uriConstructor.GetQueryUri(query)).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(parent.Serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				error.ThrowLuceneIndexNotFoundExceptionIfNedded(query);
				error.ThrowCouchCommunicationException();
			}
			using (var reader = await response.Content.ReadAsTextReaderAsync().ConfigureAwait(false))
				return LuceneQueryResultParser.Parse(reader, query);
		}

		public ISynchronousDatabaseApi Synchronously { get; private set; }

		private Task<HttpResponseMessage> StartQuery(Uri queryUri)
		{
			return parent.RequestCouchDb(new HttpRequestMessage(HttpMethod.Get, queryUri));
		}

		private static async Task<DocumentInfo> ReadDocumentInfo(HttpResponseMessage response)
		{
			dynamic couchResponse = await response.Content.ReadAsJsonObjectAsync().ConfigureAwait(false);
			var id = (string) couchResponse.id;
			var revision = (string) couchResponse.rev;

			return new DocumentInfo(id, revision);
		}
	}
}