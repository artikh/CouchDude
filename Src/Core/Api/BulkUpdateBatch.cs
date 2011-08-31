using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CouchDude.Http;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class BulkUpdateBatch: IBulkUpdateBatch
	{
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

			private IDocument document;
			public IDocument Document
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

		private void Add(UpdateDescriptor updateDescriptor)
		{
			updateDescriptors.Add(updateDescriptor);
			docIdToUpdateDescriptorMap.Add(updateDescriptor.DocumentId, updateDescriptor);
		}

		public void Create(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Revision.HasValue()) 
				throw new ArgumentException("Document seems to have been previously saved as it has a revision.", "document");
			Add(new UpdateDescriptor{ Document = document, Operation = OperationType.Create });
		}

		public void Update(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
			if (document.Revision.HasNoValue()) throw new ArgumentException("Document should have a revision to be updated", "document");
			Add(new UpdateDescriptor { Document = document, Operation = OperationType.Update });
		}

		public void Delete(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");
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
			IHttpClient httpClient, Func<HttpRequestMessage, Task<HttpResponseMessage>> startRequest, Uri databaseUri)
		{
			var bulkUpdateUri = new Uri(databaseUri, "_bulk_docs");
			var request =
				new HttpRequestMessage(HttpMethod.Post, bulkUpdateUri) { Content = new JsonContent(FormatDescriptor()) };
			return startRequest(request).ContinueWith(
				rt =>
				{
					var response = rt.Result;
					if (!response.IsSuccessStatusCode) 
						throw Errors.CreateCommunicationException(response);

					dynamic responseDescriptors = new JsonFragment(response.GetContentTextReader());
					foreach (var responseDescriptor in responseDescriptors)
					{
						string errorName = responseDescriptor.error;
						string documentId = responseDescriptor.id;
						if(errorName != null) 
							CollectError(documentId, errorName, responseDescriptor);
						else
						{
							var documentInfo = new DocumentInfo(documentId, (string)responseDescriptor.rev);
							result[documentInfo.Id] = documentInfo;
						}
					}

					switch(exceptions.Count)
					{
						case 0:
							return result;
						case 1:
							throw exceptions[0];
						default:
							throw new AggregateException("Error executing CouchDB bulk update", exceptions);
					}
				});
		}

		private void CollectError(string documentId, string errorName, dynamic responseDescriptor)
		{
			var reasonMessage = (string)responseDescriptor.reason;
			var docIdToUpdateDescriptor = docIdToUpdateDescriptorMap[documentId];
			var operation = docIdToUpdateDescriptor.Operation.ToString().ToLower();
			switch(errorName)
			{
				case "conflict":
					exceptions.Add(Errors.CreateStaleStateException(operation, documentId, docIdToUpdateDescriptor.DocumentRevision));
					break;
				case "forbidden":
					exceptions.Add(Errors.CreateInvalidDocumentException(documentId, reasonMessage));
					break;
				default:
					exceptions.Add(new CouchCommunicationException(Errors.FormatCouchError(errorName, reasonMessage)));
					break;
			}
		}

		private string FormatDescriptor()
		{
			var descriptorString = new StringBuilder("{\"docs\":[");
			foreach(var updateDescriptor in updateDescriptors)
				switch(updateDescriptor.Operation)
				{
					case OperationType.Create:
					case OperationType.Update:
						descriptorString.Append(updateDescriptor.Document.ToString()).Append(",");
						break;
					case OperationType.Delete:
						descriptorString.AppendFormat(
							@"{{""_id"":""{0}"",""_rev"":""{1}"",""_deleted"":true}},",
							updateDescriptor.DocumentId,
							updateDescriptor.DocumentRevision);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

			if (descriptorString[descriptorString.Length - 1] == ',')
				descriptorString.Remove(descriptorString.Length - 1, 1);
			descriptorString.Append("]}");
			return descriptorString.ToString();
		}
	}
}