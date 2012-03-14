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
using System.Net.Http;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class BulkUpdateBatch: IBulkUpdateBatch
	{
		private const string DeletedFlagPropertyName = "_deleted";
		private readonly DbUriConstructor uriConstructor;
		private readonly ISerializer serializer;

		private enum OperationType
		{
			Create,
			Update,
			Delete
		}

		private struct UpdateDescriptor
		{
			public OperationType Operation;
			public string DocumentId;
			public string DocumentRevision;

			private Document document;
			public Document Document
			{
				get { return document; }  
				set
				{
					document = value;
					DocumentId = document.Id;
					DocumentRevision = document.Revision;
				}
			}
		}

		private readonly IList<UpdateDescriptor> updateDescriptors = new List<UpdateDescriptor>();
		private readonly IDictionary<string, UpdateDescriptor> docIdToUpdateDescriptorMap = new Dictionary<string, UpdateDescriptor>();

		readonly IList<Exception> exceptions = new List<Exception>();
		readonly IDictionary<string, DocumentInfo> result = new Dictionary<string, DocumentInfo>();

		/// <constructor />
		public BulkUpdateBatch(DbUriConstructor uriConstructor, ISerializer serializer)
		{
			this.uriConstructor = uriConstructor;
			this.serializer = serializer;
		}

		private void Add(UpdateDescriptor updateDescriptor)
		{
			updateDescriptors.Add(updateDescriptor);
			docIdToUpdateDescriptorMap.Add(updateDescriptor.DocumentId, updateDescriptor);
		}

		public void Create(Document document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue())
				throw new ArgumentException("Document in order to be created shoud have an ID.", "document");
			if (document.Revision.HasValue()) 
				throw new ArgumentException("Document seems to have been previously saved as it has a revision.", "document");

			Add(new UpdateDescriptor{ Document = document, Operation = OperationType.Create });
		}

		public void Update(Document document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue())
				throw new ArgumentException("Document in order to be updated shoud have an ID.", "document");
			if (document.Revision.HasNoValue()) 
				throw new ArgumentException("Document should have a revision to be updated", "document");

			Add(new UpdateDescriptor { Document = document, Operation = OperationType.Update });
		}

		public void Delete(Document document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Id.HasNoValue())
				throw new ArgumentException("Document should have ID to be deleted.", "document");
			if (document.Revision.HasNoValue())
				throw new ArgumentException("Document should have a revision to be deleted", "document");
			Add(new UpdateDescriptor { Document = document, Operation = OperationType.Delete });
		}

		public void Delete(string documentId, string revision)
		{
			if (documentId.HasNoValue()) throw new ArgumentNullException("documentId");
			if (revision.HasNoValue()) throw new ArgumentNullException("revision");
			Add(new UpdateDescriptor { DocumentId = documentId, DocumentRevision = revision, Operation = OperationType.Delete });
		}

		public bool IsEmpty { get { return updateDescriptors.Count == 0; } }

		public Task<IDictionary<string, DocumentInfo>> Execute(
			Func<HttpRequestMessage, Task<HttpResponseMessage>> startRequest)
		{
			var bulkUpdateUri = uriConstructor.BulkUpdateUri;
			var request = new HttpRequestMessage(HttpMethod.Post, bulkUpdateUri) { Content = new JsonContent(FormatDescriptor()) };
			return startRequest(request).ContinueWith<Task<IDictionary<string, DocumentInfo>>>(HandleResponse).Unwrap();
		}

		private Task<IDictionary<string, DocumentInfo>> HandleResponse(Task<HttpResponseMessage> requestTask)
		{
			var response = requestTask.Result;

			if (!response.IsSuccessStatusCode)
			{
				var error = new CouchError(serializer, response);
				error.ThrowDatabaseMissingExceptionIfNedded(uriConstructor);
				error.ThrowCouchCommunicationException();
			}
			return response.Content.ReadAsJsonArrayAsync().ContinueWith<IDictionary<string, DocumentInfo>>(ProcessResponseData);
		}

		private IDictionary<string, DocumentInfo> ProcessResponseData(Task<JsonArray> readDataTask)
		{
			var responseDescriptors = readDataTask.Result.AsDynamic();
			foreach (var responseDescriptor in responseDescriptors)
			{
				string errorName = responseDescriptor.error;
				string documentId = responseDescriptor.id;
				if (errorName != null)
					CollectError(documentId, responseDescriptor.ToString());
				else
				{
					var documentInfo = new DocumentInfo(documentId, (string)responseDescriptor.rev);
					result[documentInfo.Id] = documentInfo;
				}
			}

			switch (exceptions.Count)
			{
				case 0:
					return result;
				case 1:
					throw exceptions[0];
				default:
					throw new AggregateException("Error executing CouchDB bulk update", exceptions);
			}
		}

		private void CollectError(string documentId, string errorString)
		{
			var docIdToUpdateDescriptor = docIdToUpdateDescriptorMap[documentId];
			var operation = docIdToUpdateDescriptor.Operation.ToString().ToLower();

			var error = new CouchError(serializer, errorString);

			if (error.IsConflict)
				exceptions.Add(error.CreateStaleStateException(operation, documentId, docIdToUpdateDescriptor.DocumentRevision));
			else if (error.IsForbidden)
				exceptions.Add(error.CreateInvalidDocumentException(documentId));
			else 
				exceptions.Add(error.CreateCouchCommunicationException());
		}

		private JsonObject FormatDescriptor()
		{
			var descriptor = new JsonObject();
			var array = new JsonArray();

			foreach(var updateDescriptor in updateDescriptors)
				switch(updateDescriptor.Operation)
				{
					case OperationType.Create:
					case OperationType.Update:
						array.Add(updateDescriptor.Document.RawJsonObject);
						break;
					case OperationType.Delete:
						array.Add(new JsonObject(
							new KeyValuePair<string, JsonValue>(Document.IdPropertyName, updateDescriptor.DocumentId),
							new KeyValuePair<string, JsonValue>(Document.RevisionPropertyName, updateDescriptor.DocumentRevision),
							new KeyValuePair<string, JsonValue>(DeletedFlagPropertyName, true)
						));
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			descriptor.Add("docs", array);

			return descriptor;
		}
	}
}