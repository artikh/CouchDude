using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Http;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal static class Errors
	{
		public static string ParseErrorResponseBody(HttpResponseMessage response)
		{
			return ParseErrorResponseBody(response.Content.GetTextReader(), (int)response.StatusCode);
		}

		public static string ParseErrorResponseBody(TextReader errorTextReader, int? errorCode = null)
		{
			if (errorTextReader == null) return null;

			string errorText;
			using (errorTextReader)
				errorText = errorTextReader.ReadToEnd();
			dynamic errorObject = TryGetErrorObject(errorText);
			return errorObject == null? errorText: FormatCouchError((string)errorObject.error, (string)errorObject.reason, errorCode);
		}

		public static dynamic TryGetErrorObject(string errorText)
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

		public static string FormatCouchError(string errorName, string reasonMessage, int? errorCode = null)
		{
			errorName = errorName ?? String.Empty;
			reasonMessage = reasonMessage ?? String.Empty; 
			
			var message = new StringBuilder();
			message.Append(errorName);

			if(message.Length > 0 && reasonMessage.Length > 0) message.Append(": ");
			message.Append(reasonMessage);

			if (errorCode.HasValue)
			{
				if(message.Length <= 0) 
					message.Append("Error returned by CouchDB: ").Append(errorCode);
				else 
					message.Append(" [").Append(errorCode).Append("]");
			}
			
			return message.Length > 0? message.ToString(): null;
		}

		public static CouchCommunicationException CreateCommunicationException(HttpResponseMessage response)
		{
			return new CouchCommunicationException(ParseErrorResponseBody(response) ?? "Error returned from CouchDB");
		}

		public static StaleObjectStateException CreateStaleStateException(string operation, string docId, string revision = null)
		{
			if(revision.HasValue())
				return new StaleObjectStateException("Document {0}(rev:{1}) {2} conflict detected", docId, revision, operation);
			else
				return new StaleObjectStateException("Document {0} {1} conflict detected", docId, operation);
		}

		public static  InvalidDocumentException CreateInvalidDocumentException(string docId, HttpResponseMessage response)
		{
			return CreateInvalidDocumentException(docId, ParseErrorResponseBody(response));
		}

		public static InvalidDocumentException CreateInvalidDocumentException(string docId, string reason)
		{
			return new InvalidDocumentException("Document {0} is invalid: {1}", docId, reason);
		}

		public static void ThrowIfViewRequestWasUnsuccessful(ViewQuery query, HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
				if (response.StatusCode == HttpStatusCode.NotFound)
					throw new ViewNotFoundException(
						"View {0}/{1} is not declared in database. Perhaps design document _design/{0} should be updated",
						query.DesignDocumentName,
						query.ViewName);
				else
					throw CreateCommunicationException(response);
		}

		public static void ThrowIfFulltextIndexRequestWasUnsuccessful(
			LuceneQuery query, HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
				if (response.StatusCode == HttpStatusCode.NotFound)
					throw new ViewNotFoundException(
						"Fultext index {0}/{1} is not declared in database. Perhaps design document _design/{0} should be updated",
						query.DesignDocumentName,
						query.IndexName);
				else
					throw CreateCommunicationException(response);
		}
	}
}