﻿#region Licence Info 
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
using System.Text;
using System.Threading.Tasks;
using CouchDude.Utils;
using CouchDude.Http;

namespace CouchDude.Api
{
	internal class DatabaseApi : IDatabaseApi
	{
		private readonly IHttpClient httpClient;
		private readonly string databaseName;
		private readonly Uri databaseUri;

		/// <constructor />
		public DatabaseApi(IHttpClient httpClient, Uri serverUri, string databaseName)
		{
			if (databaseName.HasNoValue()) throw new ArgumentNullException("databaseName");
			if (httpClient == null) throw new ArgumentNullException("httpClient");
			if (serverUri == null) throw new ArgumentNullException("serverUri");

			this.httpClient = httpClient;
			this.databaseName = databaseName;
			databaseUri = new Uri(serverUri, databaseName + "/");
			Synchronously = new SynchronousDatabaseApi(this);
		}

		public Task Create()
		{
			return CouchApi.StartRequest(new HttpRequestMessage(HttpMethod.Put, databaseUri), httpClient)
				.ContinueWith(
					t => {
						var response = t.Result;
						if (!response.IsSuccessStatusCode)
							new CouchError(response).ThrowCouchCommunicationException();
					});
		}

		public Task Delete()
		{
			return CouchApi.StartRequest(new HttpRequestMessage(HttpMethod.Delete, databaseUri), httpClient)
				.ContinueWith(
					t =>
					{
						var response = t.Result;
						if (!response.IsSuccessStatusCode)
						{
							var error = new CouchError(response);
							error.ThrowDatabaseMissingExceptionIfNedded(databaseName);
							error.ThrowCouchCommunicationException();
						}
					});
		}

		public Task<DatabaseInfo> RequestInfo()
		{
			return CouchApi.StartRequest(new HttpRequestMessage(HttpMethod.Get, databaseUri), httpClient)
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
						return new DatabaseInfo(exists, databaseName, responseJson);
					});
		}

		public Task<IDocument> RequestDocumentById(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			

			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Get, documentUri);
			return CouchApi.StartRequest(request, httpClient).ContinueWith<IDocument>(
				rt => {
					var response = rt.Result;
					if (!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(databaseName);
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
			

			var documentUri = GetDocumentUri(documentId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			return CouchApi.StartRequest(request, httpClient).ContinueWith(
				rt => {
					var response = rt.Result;
					if (!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(databaseName);
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
			

			var documentUri = GetDocumentUri(document.Id);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri) { Content = new JsonContent(document) };
			return CouchApi.StartRequest(request, httpClient).ContinueWith(
				rt => {
					var response = rt.Result;
					var documentId = document.Id;
					if(!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(databaseName);
						error.ThrowStaleStateExceptionIfNedded("update", documentId);
						error.ThrowInvalidDocumentExceptionIfNedded(documentId);
						error.ThrowCouchCommunicationException();
					}
					return ReadDocumentInfo(response);
				});
		}

		private static readonly IDictionary<string, DocumentInfo> EmptyDictionary = new Dictionary<string, DocumentInfo>(0);

		public Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder)
		{
			if (updateCommandBuilder == null) throw new ArgumentNullException("updateCommandBuilder");
			
			var unitOfWork = new BulkUpdateBatch(databaseName);
			updateCommandBuilder(unitOfWork);
			return unitOfWork.IsEmpty 
				? Task.Factory.StartNew(() => EmptyDictionary) 
				: unitOfWork.Execute(httpClient, request => CouchApi.StartRequest(request, httpClient), databaseUri);
		}

		public Task<string> RequestLastestDocumentRevision(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			
			var documentUri = GetDocumentUri(docId);
			var request = new HttpRequestMessage(HttpMethod.Head, documentUri);
			return CouchApi.StartRequest(request, httpClient).ContinueWith(
				rt => {
					var response = rt.Result;
					
					if (!response.IsSuccessStatusCode)
					{
						var couchApiError = new CouchError(response);
						couchApiError.ThrowDatabaseMissingExceptionIfNedded(databaseName);
						couchApiError.ThrowStaleStateExceptionIfNedded("update", docId);
						couchApiError.ThrowInvalidDocumentExceptionIfNedded(docId);
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


			return StartQuery(query.ToUri()).ContinueWith(
				rt => {
					var response = rt.Result;
					if(!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(databaseName);
						error.ThrowViewNotFoundExceptionIfNedded(query);
						error.ThrowCouchCommunicationException();
					}
					return ViewQueryResultParser.Parse(rt.Result.GetContentTextReader(), query);
				});
		}

		public Task<ILuceneQueryResult> QueryLucene(LuceneQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			
			return StartQuery(query.ToUri()).ContinueWith(
				rt => {
					var response = rt.Result;
					if (!response.IsSuccessStatusCode)
					{
						var error = new CouchError(response);
						error.ThrowDatabaseMissingExceptionIfNedded(databaseName);
						error.ThrowLuceneIndexNotFoundExceptionIfNedded(query);
						error.ThrowCouchCommunicationException();
					}
					return LuceneQueryResultParser.Parse(rt.Result.GetContentTextReader(), query);
				});
		}

		public ISynchronousDatabaseApi Synchronously { get; private set; }

		private Task<HttpResponseMessage> StartQuery(Uri uri)
		{
			var viewUri = new Uri(databaseUri, uri);
			var request = new HttpRequestMessage(HttpMethod.Get, viewUri);
			var task = CouchApi.StartRequest(request, httpClient);
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

		private static DocumentInfo ReadDocumentInfo(HttpResponseMessage response)
		{
			dynamic couchResponse = new JsonFragment(response.GetContentTextReader());
			var id = (string)couchResponse.id;
			var revision = (string)couchResponse.rev;

			return new DocumentInfo(id, revision);
		}
	}
}