
using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CouchDude.Core.Utils;

namespace CouchDude.Core.Api
{
	internal partial class CouchApi
	{
		public IDocument RequestDocumentByIdAndWaitForResult(string docId)
		{
			Contract.Requires(docId.HasValue());
			return WaitForResult(RequestDocumentById(docId));
		}

		public IJsonFragment DeleteDocumentAndWaitForResult(string docId, string revision)
		{
			Contract.Requires(docId.HasValue());
			Contract.Requires(!revision.HasValue());

			return WaitForResult(DeleteDocument(docId, revision));
		}

		public IJsonFragment SaveDocumentSyncAndWaitForResult(IDocument document)
		{
			Contract.Requires(document != null);
			Contract.Requires(!document.Id.HasNoValue());

			return WaitForResult(SaveDocument(document));
		}

		public IJsonFragment UpdateDocumentAndWaitForResult(IDocument document)
		{
			Contract.Requires(document != null);
			Contract.Requires(document.Id.HasValue());

			return WaitForResult(UpdateDocument(document));
		}

		public string RequestLastestDocumentRevisionAndWaitForResult(string docId)
		{
			Contract.Requires(docId.HasValue());
			return WaitForResult(RequestLastestDocumentRevision(docId));
		}

		/// <inheritdoc/>
		public IPagedList<ViewResultRow> QueryAndWaitForResult(ViewQuery query)
		{
			Contract.Requires(query != null);
			Contract.Requires(query.Skip < 10);

			return WaitForResult(Query(query));
		}

		/// <inheritdoc/>
		/// TODO: Add result weight to result
		public IPagedList<LuceneResultRow> QueryLuceneAndWaitForResult(LuceneQuery query)
		{
			Contract.Requires(query != null);
			return WaitForResult(QueryLucene(query));
		}

		private static T WaitForResult<T>(Task<T> task)
		{
			try
			{
				task.Wait();
				return task.Result;
			}
			catch(AggregateException e)
			{
				if(e.InnerExceptions.Count == 1)
				{
					var singleInnerExcepion = e.InnerException;
					PreserveStackTrace(singleInnerExcepion);
					throw singleInnerExcepion;
				}
				throw;
			}
		}

		private static void PreserveStackTrace(Exception e)
		{
			var streamingContext = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var objectManager = new ObjectManager(null, streamingContext);
			var serializationInfo = new SerializationInfo(e.GetType(), new FormatterConverter());

			e.GetObjectData(serializationInfo, streamingContext);
			objectManager.RegisterObject(e, 1, serializationInfo); // prepare for SetObjectData
			objectManager.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString
		}
	}
}