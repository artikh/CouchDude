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
using System.Net;
using System.Net.Http;
using System.Text;
using CouchDude.Http;
using CouchDude.Utils;
using JetBrains.Annotations;

namespace CouchDude.Api
{
	/// <summary>Represents CouchDB error.</summary>
	internal struct CouchError
	{
		public const string Conflict = "conflict";
		public const string Forbidden = "forbidden";
		public const string NoDbFile = "no_db_file";
		public const string NotFound = "not_found";
		public const string FileExists = "file_exists";

		public readonly string Error;
		public readonly string Reason;
		public readonly HttpStatusCode? HttpStatusCode;

		public CouchError(dynamic responseObject)
		{
			Error = Reason = String.Empty;
			HttpStatusCode = null;

			if (responseObject != null) 
				UpdateUsingErrorDescriptor(responseObject, ref Error, ref Reason);
		}

		public CouchError(HttpResponseMessage response)
		{
			if(response == null) throw new ArgumentNullException("response");
			if(response.IsSuccessStatusCode)
				throw new ArgumentException("Successfull response message could not be basis of the error object.", "response");

			HttpStatusCode = response.StatusCode;

			// last resort values
			Error = response.StatusCode.ToString();
			Reason = response.ReasonPhrase;

			if (response.Content == null) return;

			string responseText;
			using (var reader = response.Content.GetTextReader())
				responseText = reader.ReadToEnd();

			var responseObject = TryGetResponseObject(responseText);

			if (responseObject == null)
				Reason = responseText;
			else
				UpdateUsingErrorDescriptor(responseObject, ref Error, ref Reason);
		}

		private static void UpdateUsingErrorDescriptor(dynamic responseObject, ref string error, ref string reason)
		{
			string errorMessage = responseObject.error;
			if (!String.IsNullOrWhiteSpace(errorMessage))
				error = errorMessage;

			string reasonMessage = responseObject.reason;
			if (!String.IsNullOrWhiteSpace(reasonMessage))
				reason = reasonMessage;
		}

		private static dynamic TryGetResponseObject(string responseText)
		{
			try
			{
				return new JsonFragment(responseText);
			}
			catch (Exception)
			{
				return null;
			}
		}

		[TerminatesProgram]
		public Exception ThrowCouchCommunicationException() { throw CreateCouchCommunicationException(); }

		public CouchCommunicationException CreateCouchCommunicationException() { return new CouchCommunicationException(ToString()); }

		[TerminatesProgram]
		public StaleObjectStateException ThrowStaleStateException(string operation, string docId, string revision = null) 
		{
			throw CreateStaleStateException(operation, docId, revision);
		}

		public Exception CreateStaleStateException(string operation, string docId, string revision)
		{
			if (revision.HasValue())
				return new StaleObjectStateException(
					"Document {0}(rev:{1}) {2} conflict detected", docId, revision, operation);
			else
				return new StaleObjectStateException("Document {0} {1} conflict detected", docId, operation);
		}

		[TerminatesProgram]
		public InvalidDocumentException ThrowInvalidDocumentException(string docId)
		{
			throw CreateInvalidDocumentException(docId);
		}

		public InvalidDocumentException CreateInvalidDocumentException(string docId)
		{
			return new InvalidDocumentException("Document {0} is invalid: {1}", docId, ToString());
		}

		[TerminatesProgram]
		public InvalidDocumentException ThrowViewNotFoundException(ViewQuery query)
		{
			throw new ViewNotFoundException(query);
		}

		[TerminatesProgram]
		public InvalidDocumentException ThrowLuceneIndexNotFoundException(LuceneQuery query)
		{
			throw new LuceneIndexNotFoundException(query);
		}

		public override string ToString() 
		{
			var errorName = Error;
			var reasonMessage = Reason;

			var message = new StringBuilder();
			message.Append(errorName);

			if (message.Length > 0 && reasonMessage.Length > 0) message.Append(": ");
			message.Append(reasonMessage);

			if (HttpStatusCode.HasValue)
			{
				if (message.Length <= 0)
					message.Append("Error returned by CouchDB: ").Append((int) HttpStatusCode);
				else
					message.Append(" [").Append((int)HttpStatusCode).Append("]");
			}

			return message.Length > 0 ? message.ToString() : "Error returned from CouchDB";
		}
	}
}