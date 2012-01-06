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
using CouchDude.Api;

namespace CouchDude
{
	/// <summary>Synchronous version of databes-level APIs.</summary>
	public interface ISynchronousDatabaseApi
	{
		/// <summary>Requests CouchDB for document using <paramref name="documentId"/> 
		/// and <paramref name="revision"/> and waits for result of the operation.</summary>
		Document RequestDocument(
			string documentId,
			string revision = null,
			AdditionalDocumentProperty additionalProperties = default(AdditionalDocumentProperty));

		/// <summary>Saves new document to CouchDB and waits for the result of the operation.</summary>
		DocumentInfo SaveDocument(Document document);

		/// <summary>Saves new document to CouchDB detecting conflicts and resolving them by discarding 
		/// remote version of document in favour of local one. Client code waits on result.</summary>
		DocumentInfo SaveDocument(Document document, bool overwriteConcurrentUpdates);
		
		/// <summary>Creates new document by copying another document's content.</summary>
		DocumentInfo CopyDocument(string originalDocumentId, string originalDocumentRevision, string targetDocumentId, string targetDocumentRevision = null);

		/// <summary>Retrives current document revision from CouchDB and waits for the result of the operation. </summary>
		/// <remarks><c>null</c> returned if there is no such document in database.</remarks>
		string RequestLastestDocumentRevision(string docId);

		/// <summary>Deletes document of provided <paramref name="docId"/> if it's revision
		/// is equal to provided <paramref name="revision"/> and waits for the result of the operation.</summary>
		DocumentInfo DeleteDocument(string docId, string revision);
		
		/// <summary>Requests document attachment directly from database.</summary>
		Attachment RequestAttachment(string attachmentId, string documentId, string documentRevision = null);

		/// <summary>Saves document attachment directly to database. If <paramref name="documentRevision"/> is <c>null</c>
		/// creates new document for attachment.</summary>
		DocumentInfo SaveAttachment(Attachment attachment, string documentId, string documentRevision = null);

		/// <summary>Requests document attachment directly from database.</summary>
		DocumentInfo DeleteAttachment(string attachmentId, string documentId, string documentRevision);

		/// <summary>Creates, updates and deletes several documents as a whole from database. </summary>
		IDictionary<string, DocumentInfo> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder);

		/// <summary>Queries CouchDB view and waits for the result of the operation.</summary>
		IViewQueryResult Query(ViewQuery query);

		/// <summary>Queries couchdb-lucene index and waits for the result of the operation.</summary>
		ILuceneQueryResult QueryLucene(LuceneQuery query);

		/// <summary>Demands database to be created and waits for result of the operation.</summary>
		void Create(bool throwIfExists = true);

		/// <summary>Demands database to be deleted and waits for result of the operation.</summary>
		void Delete();

		/// <summary>Demands database status information and waits for result of the operation.</summary>
		DatabaseInfo RequestInfo();
	}
}