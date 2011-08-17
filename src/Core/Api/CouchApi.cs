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
using System.IO;
using System.Linq;
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
					ThrowIfNotOk(response);
					return new Document(response.GetContentTextReader());
				});
		}

		public Task<DocumentInfo> DeleteDocument(string docId, string revision)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			if (string.IsNullOrEmpty(revision)) throw new ArgumentNullException("revision");
			

			var documentUri = GetDocumentUri(docId, revision);
			var request = new HttpRequestMessage(HttpMethod.Delete, documentUri);
			return StartRequest(request).ContinueWith(
				rt => {
					var response = rt.Result;
					ThrowIfNotOk(response);
					return ReadDocumentInfo(response);
				});
		}

		public Task<DocumentInfo> SaveDocument(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue()) 
				throw new ArgumentException("Document ID should not be empty or noll.", "document");
			

			var documentUri = GetDocumentUri(document.Id);
			var request = new HttpRequestMessage(HttpMethod.Put, documentUri);
			request.SetStringContent(document.ToString());
			return StartRequest(request).ContinueWith(
				rt => {
					var response = rt.Result;
					ThrowIfNotOk(response);
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

			var bulkUpdateUri = new Uri(databaseUri, "_bulk_docs");
			var request = new HttpRequestMessage(HttpMethod.Post, bulkUpdateUri);
			request.SetStringContent(unitOfWork.UpdateDescriptor);
			return StartRequest(request).ContinueWith(
				rt => {
					var response = rt.Result;
					ThrowIfNotOk(response);

					var exceptions = new List<Exception>();
					IDictionary<string, DocumentInfo> result = new Dictionary<string, DocumentInfo>();
					dynamic responseDescriptors = new JsonFragment(response.GetContentTextReader());
					foreach(var responseDescriptor in responseDescriptors)
					{
						string error = responseDescriptor.error;
						if (error != null) 
							exceptions.Add(CreateException(error, (string)responseDescriptor.reason));
						else
						{
							var documentInfo = new DocumentInfo((string)responseDescriptor.id, (string)responseDescriptor.rev);
							result[documentInfo.Id] = documentInfo;
						}
					}

					return result;
				});
		}

		public Exception CreateException(string errorName, string reasonMessage, int? errorCode = null)
		{
			switch (errorName)
			{
				case "conflict":
					return CreateStaleStateException();
				case "forbidden":
					return CreateInvalidDocumentException(reasonMessage);
				default:
					return new CouchCommunicationException(FormatCouchError(errorName, reasonMessage, errorCode));
			}
		}

		public Task<string> RequestLastestDocumentRevision(string docId)
		{
			if (string.IsNullOrEmpty(docId)) throw new ArgumentNullException("docId");
			
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
			

			return StartQuery(query.ToUri()).ContinueWith<IPagedList<ViewResultRow>>(
				rt => ViewResultParser.Parse(rt.Result.GetContentTextReader(), query)
			);
		}

		public Task<IPagedList<LuceneResultRow>> QueryLucene(LuceneQuery query)
		{
			if (query == null) throw new ArgumentNullException("query");
			

			return StartQuery(query.ToUri()).ContinueWith<IPagedList<LuceneResultRow>>(
				rt => LuceneResultParser.Parse(rt.Result.GetContentTextReader(), query)
			);
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

		static string ParseErrorResponseBody(HttpResponseMessage response)
		{
			return ParseErrorResponseBody(response.Content.GetTextReader(), (int)response.StatusCode);
		}

		internal static string ParseErrorResponseBody(TextReader errorTextReader, int? errorCode = null)
		{
			if (errorTextReader == null) return null;

			string errorText;
			using (errorTextReader)
				errorText = errorTextReader.ReadToEnd();
			dynamic errorObject = TryGetErrorObject(errorText);
			return errorObject == null? errorText: FormatCouchError((string)errorObject.error, (string)errorObject.reason, errorCode);
		}

		private static dynamic TryGetErrorObject(string errorText)
		{
			try
			{
				return new JsonFragment(errorText);
			}
			catch (Exception)
			{
				return null;
			}
		}
		
		private static string FormatCouchError(string errorName, string reasonMessage, int? errorCode = null)
		{
			errorName = errorName ?? string.Empty;
			reasonMessage = reasonMessage ?? string.Empty; 
			
			var message = new StringBuilder();
			message.Append(errorName);

			if(message.Length > 0 && reasonMessage.Length > 0) message.Append(": ");
			message.Append(reasonMessage);

			if (errorCode.HasValue)
			{
				if(message.Length <= 0) message.Append("Error returned by CouchDB: ").Append(errorCode);
				else message.Append("[").Append(errorCode).Append("]");
			}

			return message.Length > 0? message.ToString(): null;
		}

		private static void ThrowIfNotOk(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode) return;

			switch(response.StatusCode)
			{
				case HttpStatusCode.Conflict:
					throw CreateStaleStateException();
				case HttpStatusCode.Forbidden:
					throw CreateInvalidDocumentException(ParseErrorResponseBody(response));
				default:
					throw CreateCommunicationException(response);
			}
		}

		private static CouchCommunicationException CreateCommunicationException(HttpResponseMessage response)
		{
			return new CouchCommunicationException(
				string.Concat(ParseErrorResponseBody(response) ?? "Error returned from CouchDB", " ", (int)response.StatusCode)
			);
		}

		private static StaleObjectStateException CreateStaleStateException()
		{
			return new StaleObjectStateException("Document update conflict detected");
		}

		private static InvalidDocumentException CreateInvalidDocumentException(string reason)
		{
			return new InvalidDocumentException("Document is invalid: " + reason);
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

		private static DocumentInfo ReadDocumentInfo(HttpResponseMessage response)
		{
			dynamic couchResponse = new JsonFragment(response.GetContentTextReader());
			var id = (string)couchResponse.id;
			var revision = (string)couchResponse.rev;

			return new DocumentInfo(id, revision);
		}

		class BulkUpdateUnitOfWork: IBulkUpdateUnitOfWork
		{
			private readonly IList<string> updateDescriptors = new List<string>();

			public void Create(IDocument document)
			{
				if (document == null) throw new ArgumentNullException("document");
				if (document.Revision.HasValue()) 
					throw new ArgumentException("Document seems to have been previously saved as it has a revision.", "document");
				updateDescriptors.Add(document.ToString());
			}

			public void Update(IDocument document)
			{
				if (document == null) throw new ArgumentNullException("document");
				if (document.Revision.HasNoValue()) throw new ArgumentException("Document should have a revision to be updated", "document");
				updateDescriptors.Add(document.ToString());
			}

			public void Delete(IDocument document)
			{
				if (document == null) throw new ArgumentNullException("document");
				if (document.Revision.HasNoValue())
					throw new ArgumentException("Document should have a revision to be deleted", "document");
				Delete(document.Id, document.Revision);
			}

			public void Delete(string id, string revision)
			{
				if (id.HasNoValue()) throw new ArgumentNullException("id");
				if (revision.HasNoValue()) throw new ArgumentNullException("revision");
				updateDescriptors.Add(
					string.Format("{{\"_id\":\"{0}\",\"_rev\":\"{1}\",\"_deleted\":true}}", id, revision)
				);
			}

			public bool IsEmpty { get { return updateDescriptors.Count == 0; } }

			public string UpdateDescriptor
			{
				get
				{
					var descripton = new StringBuilder("[");
					foreach(var updateDescriptor in updateDescriptors) 
						descripton.Append(updateDescriptor).Append(",");
					if (descripton[descripton.Length - 1] == ',')
						descripton.Remove(descripton.Length - 1, 1);
					descripton.Append("]");
					return descripton.ToString();
				}
			}
		}
	}
}