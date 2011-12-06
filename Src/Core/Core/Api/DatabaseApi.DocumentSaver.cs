using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDude.Api
{
	partial class DatabaseApi
	{
		private class DocumentSaver
		{
			private const int DefaultConflictUpdateAttemps = 7;

			private readonly DatabaseApi parent;
			private readonly IDocument document;
			private volatile int overwriteAttemptsLeft = DefaultConflictUpdateAttemps;
			private readonly Func<CouchError, DocumentInfo> conflictAction;
			private readonly TaskCompletionSource<DocumentInfo> completionSource;

			private DocumentSaver(DatabaseApi parent, IDocument document, bool overwriteConcurrentUpdates)
			{
				this.parent = parent;
				this.document = document;
				if (overwriteConcurrentUpdates)
				{
					conflictAction = ReturnDefault;
					completionSource = new TaskCompletionSource<DocumentInfo>();
				}
				else conflictAction = ThrowConflict;
			}

			public static Task<DocumentInfo> StartSaving(DatabaseApi parent, IDocument document, bool overwriteConcurrentUpdates)
			{
				var saver = new DocumentSaver(parent, document, overwriteConcurrentUpdates);
				return overwriteConcurrentUpdates ? saver.SaveOverriding() : saver.Save();
			}

			private void HandleAttempt(Task<DocumentInfo> attemptTask)
			{
				if (attemptTask.IsFaulted)
					// ReSharper disable PossibleNullReferenceException
					completionSource.TrySetException(attemptTask.Exception.InnerExceptions);
					// ReSharper restore PossibleNullReferenceException
				else if (attemptTask.IsCanceled)
					completionSource.TrySetCanceled();
				else if (!attemptTask.Result.Equals(default(DocumentInfo))) // i.e. no conflict detected 
					completionSource.TrySetResult(attemptTask.Result);
				else if (--overwriteAttemptsLeft <= 0)
					completionSource.TrySetException(
						new RaceConditionDetectedException(document.Id, document.Revision, DefaultConflictUpdateAttemps));
				else
					parent.RequestLastestDocumentRevision(document.Id)
						.ContinueWith(
							rrt => {
								document.Revision = rrt.Result;
								Save().ContinueWith(HandleAttempt);
							});
			}

			private static DocumentInfo ReturnDefault(CouchError error) { return default(DocumentInfo); }
			private DocumentInfo ThrowConflict(CouchError error)
			{
				throw error.CreateStaleStateException("update", document.Id);
			}

			private Task<DocumentInfo> SaveOverriding()
			{
				Save().ContinueWith(HandleAttempt);
				return completionSource.Task;
			}

			private async Task<DocumentInfo> Save()
			{
				var request = new HttpRequestMessage(
					HttpMethod.Put, 
					parent.uriConstructor.GetFullDocumentUri(document.Id)
				) {
					Content = new JsonContent(document)
				};

				var response = await parent.parent.RequestCouchDb(request);
				var documentId = document.Id;
				if (!response.IsSuccessStatusCode)
				{
					var error = new CouchError(response);
					error.ThrowDatabaseMissingExceptionIfNedded(parent.uriConstructor);
					if (error.IsConflict)
						return conflictAction(error);
					error.ThrowInvalidDocumentExceptionIfNedded(documentId);
					error.ThrowCouchCommunicationException();
				}
				return await ReadDocumentInfo(response);
			}
		}
	}
}
