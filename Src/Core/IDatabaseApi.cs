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
using System.Threading.Tasks;

namespace CouchDude
{
	/// <summary>Represents low-level CouchDB API.</summary>
	public interface IDatabaseApi
	{
		/// <summary>Requests CouchDB for document using <paramref name="documentId"/> 
		/// and <paramref name="revision"/>.</summary>
		Task<IDocument> RequestDocument(string documentId, string revision = null);

		/// <summary>Saves new document to CouchDB.</summary>
		Task<DocumentInfo> SaveDocument(IDocument document);

		/// <summary>Creates new document by copying another document's content.</summary>
		Task<DocumentInfo> CopyDocument(
			string originalDocumentId,
			string targetDocumentId,
			string originalDocumentRevision = null, 
			string targetDocumentRevision = null);

		/// <summary>Requests document attachment directly from database.</summary>
		Task<IDocumentAttachment> RequestAttachment(string attachmentId, string documentId, string documentRevision = null);

		/// <summary>Saves document attachment directly to database. If <paramref name="documentRevision"/> is <c>null</c>
		/// creates new document for attachment.</summary>
		Task<DocumentInfo> SaveAttachment(IDocumentAttachment attachment, string documentId, string documentRevision = null);

		/// <summary>Requests document attachment directly from database.</summary>
		Task<DocumentInfo> DeleteAttachment(string attachmentId, string documentId, string documentRevision = null);

		/// <summary>Retrives current document revision from CouchDB. </summary>
		/// <remarks><c>null</c> returned if there is no such document in database.</remarks>
		Task<string> RequestLastestDocumentRevision(string documentId);

		/// <summary>Deletes document of provided <paramref name="documentId"/> if it's revision
		/// is equal to provided <paramref name="revision"/> from CouchDB.</summary>
		Task<DocumentInfo> DeleteDocument(string documentId, string revision);

		/// <summary>Queries CouchDB view.</summary>
		Task<IViewQueryResult> Query(ViewQuery query);

		/// <summary>Queries couchdb-lucene index.</summary>
		Task<ILuceneQueryResult> QueryLucene(LuceneQuery query);

		/// <summary>Creates, updates and deletes several documents as a whole in CouchDB. </summary>
		Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder);

		/// <summary>Demands database to be created.</summary>
		Task Create();

		/// <summary>Demands database to be deleted.</summary>
		Task Delete();

		/// <summary>Demands database status information.</summary>
		Task<DatabaseInfo> RequestInfo();
		
		/// <summary>Synchronous version of databes-level APIs.</summary>
		ISynchronousDatabaseApi Synchronously { get; }
	}
}