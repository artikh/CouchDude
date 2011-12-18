#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
		public const string Conflict          = "conflict";
		public const string Forbidden         = "forbidden";
		public const string NoDbFile          = "no_db_file";
		public const string NotFound          = "not_found";
		public const string Missing           = "missing";
		public const string MissingNamedView  = "missing_named_view";
		public const string FileExists        = "file_exists";
		public const string AttachmentMissing = "Document is missing attachment";

		public readonly string Error;
		public readonly string Reason;
		public readonly HttpStatusCode? StatusCode;

		public CouchError(ISerializer serializer, string responseString)
		{
			Error = Reason = String.Empty;
			StatusCode = null;

			if (responseString.HasValue()) 
				UpdateUsingErrorDescriptor(serializer, responseString, ref Error, ref Reason);
		}

		public CouchError(ISerializer serializer, HttpResponseMessage response)
		{
			if(response == null) throw new ArgumentNullException("response");
			if(response.IsSuccessStatusCode)
				throw new ArgumentException("Successfull response message could not be basis of the error object.", "response");

			StatusCode = response.StatusCode;

			// last resort values
			// ReSharper disable SpecifyACultureInStringConversionExplicitly
			Error = response.StatusCode.ToString();
			// ReSharper restore SpecifyACultureInStringConversionExplicitly
			Reason = response.ReasonPhrase;

			if (response.Content == null) return;

			var responseText = response.Content.ReadAsStringAsync().Result;
			UpdateUsingErrorDescriptor(serializer, responseText, ref Error, ref Reason);
		}

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
		public Exception ThrowCouchCommunicationException() { throw CreateCouchCommunicationException(); }

		public CouchCommunicationException CreateCouchCommunicationException() { return new CouchCommunicationException(ToString()); }

		public void ThrowStaleStateExceptionIfNedded(string operation, string docId, string revision = null) 
		{
			if (IsConflict)
				throw CreateStaleStateException(operation, docId, revision);
		}

		public bool IsConflict { get { return StatusCode == HttpStatusCode.Conflict; } }

		public Exception CreateStaleStateException(string operation, string docId, string revision = null)
		{
			if (revision.HasValue())
				return new StaleObjectStateException(
					"Document {0}(rev:{1}) {2} conflict detected", docId, revision, operation);
			return new StaleObjectStateException("Document {0} {1} conflict detected", docId, operation);
		}

		public void ThrowInvalidDocumentExceptionIfNedded(string docId)
		{
			if (StatusCode == HttpStatusCode.Forbidden)
				throw CreateInvalidDocumentException(docId);
		}

		public InvalidDocumentException CreateInvalidDocumentException(string docId)
		{
			return new InvalidDocumentException("Document {0} is invalid: {1}", docId, ToString());
		}

		public void ThrowViewNotFoundExceptionIfNedded(ViewQuery query)
		{
			if (StatusCode == HttpStatusCode.NotFound && Reason == MissingNamedView)
				throw new ViewNotFoundException(query);
		}

		public void ThrowLuceneIndexNotFoundExceptionIfNedded(LuceneQuery query)
		{
			if (StatusCode == HttpStatusCode.NotFound && Reason == Missing)
				throw new LuceneIndexNotFoundException(query);
		}

		public void ThrowDocumentNotFoundIfNedded(string documentId, string revisionId)
		{
			if (StatusCode == HttpStatusCode.NotFound && Reason == Missing)
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

		public bool IsAttachmentMissingFromDocument { get { return StatusCode == HttpStatusCode.NotFound && Reason == AttachmentMissing; } }

		public bool IsDatabaseMissing { get { return Error == NotFound && Reason == NoDbFile; } }

		public override string ToString() 
		{
			var errorName = Error;
			var reasonMessage = Reason;

			var message = new StringBuilder();
			message.Append(errorName);

			if (message.Length > 0 && reasonMessage.Length > 0) message.Append(": ");
			message.Append(reasonMessage);

			if (StatusCode.HasValue)
			{
				if (message.Length <= 0)
					message.Append("Error returned by CouchDB: ").Append((int) StatusCode);
				else
					message.Append(" [").Append((int)StatusCode).Append("]");
			}

			return message.Length > 0 ? message.ToString() : "Error returned from CouchDB";
		}
	}
}