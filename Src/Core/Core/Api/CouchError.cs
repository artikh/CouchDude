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
using System.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude.Api
{
	/// <summary>Represents CouchDB error.</summary>
	internal struct CouchError
	{
		const string Conflict          = "conflict";
		const string Forbidden         = "forbidden";
		const string NoDbFile          = "no_db_file";
		const string NotFound          = "not_found";
		const string Missing           = "missing";
		const string MissingNamedView  = "missing_named_view";
		const string FileExists        = "file_exists";
		const string AttachmentMissing = "Document is missing attachment";

		readonly string error;
		readonly string reason;
		readonly HttpStatusCode? statusCode;

		public CouchError(ISerializer serializer, string responseString)
		{
			error = reason = String.Empty;
			statusCode = null;

			if (responseString.HasValue()) 
				UpdateUsingErrorDescriptor(serializer, responseString, ref error, ref reason);
		}

		public CouchError(ISerializer serializer, HttpResponseMessage response)
		{
			if(response == null) throw new ArgumentNullException("response");
			if(response.IsSuccessStatusCode)
				throw new ArgumentException("Successfull response message could not be basis of the error object.", "response");

			statusCode = response.StatusCode;

			// last resort values
			// ReSharper disable SpecifyACultureInStringConversionExplicitly
			error = response.StatusCode.ToString();
			// ReSharper restore SpecifyACultureInStringConversionExplicitly
			reason = response.ReasonPhrase;

			if (response.Content == null) return;

			var responseText = response.Content.ReadAsStringAsync().Result;
			UpdateUsingErrorDescriptor(serializer, responseText, ref error, ref reason);
		}

		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		internal class CouchErrorDescriptor
		{
			public string Error;
			public string Reason;
		}

		private static void UpdateUsingErrorDescriptor(
			ISerializer serializer, string responseString, ref string error, ref string reason)
		{
			var couchErrorDescriptorJsonFragment = TryGetResponseObject(responseString);
			if(couchErrorDescriptorJsonFragment != null)
			{
				var couchErrorDescriptor = 
					serializer.ConvertFromJson<CouchErrorDescriptor>(couchErrorDescriptorJsonFragment, throwOnError: false);
				if(couchErrorDescriptor != null)
				{
					if (!String.IsNullOrWhiteSpace(couchErrorDescriptor.Error))
						error = couchErrorDescriptor.Error;

					if (!String.IsNullOrWhiteSpace(couchErrorDescriptor.Reason))
						reason = couchErrorDescriptor.Reason;

					return;
				}
			}

			reason = responseString;
		}

		private static JsonValue TryGetResponseObject(string responseText)
		{
			try
			{
				return JsonValue.Parse(responseText);
			}
			catch (Exception)
			{
				return null;
			}
		}

		[TerminatesProgram]
		public void ThrowCouchCommunicationException() { throw CreateCouchCommunicationException(); }

		public CouchCommunicationException CreateCouchCommunicationException() { return new CouchCommunicationException(ToString()); }

		public void ThrowStaleStateExceptionIfNedded(string operation, string docId, string revision = null) 
		{
			if (IsConflict)
				throw CreateStaleStateException(operation, docId, revision);
		}

		public bool IsConflict { get { return error == Conflict; } }

		public bool IsForbidden { get { return error == Forbidden; } }

		public Exception CreateStaleStateException(string operation, string docId, string revision = null)
		{
			if (revision.HasValue())
				return new StaleObjectStateException(
					"Document {0}(rev:{1}) {2} conflict detected", docId, revision, operation);
			return new StaleObjectStateException("Document {0} {1} conflict detected", docId, operation);
		}

		public void ThrowStaleStateExceptionForDocumentCopyIfNedded(object originalDocumentId, object originalDocumentRevision, object targetDocumentId, object targetDocumentRevision)
		{
			if(IsConflict)
				throw new StaleObjectStateException(
					"Document {0}(rev:{1}) to {2}(rev:{3}) copy conflict detected",
					originalDocumentId,
					originalDocumentRevision,
					targetDocumentId,
					targetDocumentRevision
				);
		}

		public void ThrowInvalidDocumentExceptionIfNedded(string docId)
		{
			if (statusCode == HttpStatusCode.Forbidden)
				throw CreateInvalidDocumentException(docId);
		}

		public InvalidDocumentException CreateInvalidDocumentException(string docId)
		{
			return new InvalidDocumentException("Document {0} is invalid: {1}", docId, ToString());
		}

		public void ThrowViewNotFoundExceptionIfNedded(ViewQuery query)
		{
			if (statusCode == HttpStatusCode.NotFound && reason == MissingNamedView)
				throw new ViewNotFoundException(query);
		}

		public void ThrowLuceneIndexNotFoundExceptionIfNedded(LuceneQuery query)
		{
			if (statusCode == HttpStatusCode.NotFound && reason == Missing)
				throw new LuceneIndexNotFoundException(query);
		}

		public void ThrowDocumentNotFoundIfNedded(string documentId, string revisionId)
		{
			if (statusCode == HttpStatusCode.NotFound && reason == Missing)
				throw new DocumentNotFoundException(documentId, revisionId);
		}

		public void ThrowDatabaseMissingExceptionIfNedded(DbUriConstructor uriConstructor)
		{
			ThrowDatabaseMissingExceptionIfNedded(uriConstructor.DatabaseName);
		}

		public void ThrowDatabaseMissingExceptionIfNedded(string databaseName)
		{
			if (IsDatabaseMissing)
				throw new DatabaseMissingException(databaseName);
		}

		public void ThrowAttachmentMissingException(string attachmentId, string documentId, string documentRevision = null)
		{
			if (IsAttachmentMissingFromDocument)
				throw new DocumentAttachmentMissingException(attachmentId, documentId, documentRevision);
		}

		public bool IsAttachmentMissingFromDocument { get { return statusCode == HttpStatusCode.NotFound && reason == AttachmentMissing; } }

		public bool IsDatabaseMissing { get { return error == NotFound && reason == NoDbFile; } }

		public bool IsAlreadyDatabaseExists
		{
			get { return error == FileExists && statusCode == HttpStatusCode.PreconditionFailed; }
		}

		public override string ToString() 
		{
			var errorName = error;
			var reasonMessage = reason;

			var message = new StringBuilder();
			message.Append(errorName);

			if (message.Length > 0 && reasonMessage.Length > 0) message.Append(": ");
			message.Append(reasonMessage);

			if (statusCode.HasValue)
			{
				if (message.Length <= 0)
					message.Append("Error returned by CouchDB: ").Append((int) statusCode);
				else
					message.Append(" [").Append((int)statusCode).Append("]");
			}

			return message.Length > 0 ? message.ToString() : "Error returned from CouchDB";
		}
	}
}