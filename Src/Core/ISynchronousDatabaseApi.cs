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
using System.Collections.Generic;

namespace CouchDude
{
	/// <summary>Synchronous version of databes-level APIs.</summary>
	public interface ISynchronousDatabaseApi
	{
		/// <summary>Requests CouchDB for document using <paramref name="docId"/> 
		/// and <paramref name="revision"/> and waits for result of the operation.</summary>
		IDocument RequestDocument(string docId, string revision = null);

		/// <summary>Saves new document to CouchDB and waits for the result of the operation.</summary>
		DocumentInfo SaveDocument(IDocument document);

		/// <summary>Retrives current document revision from CouchDB and waits for the result of the operation. </summary>
		/// <remarks><c>null</c> returned if there is no such document in database.</remarks>
		string RequestLastestDocumentRevision(string docId);

		/// <summary>Deletes document of provided <paramref name="docId"/> if it's revision
		/// is equal to provided <paramref name="revision"/> and waits for the result of the operation.</summary>
		DocumentInfo DeleteDocument(string docId, string revision);

		/// <summary>Creates, updates and deletes several documents as a whole from database. </summary>
		IDictionary<string, DocumentInfo> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder);

		/// <summary>Queries CouchDB view and waits for the result of the operation.</summary>
		IViewQueryResult Query(ViewQuery query);

		/// <summary>Queries couchdb-lucene index and waits for the result of the operation.</summary>
		ILuceneQueryResult QueryLucene(LuceneQuery query);

		/// <summary>Demands database to be created and waits for result of the operation.</summary>
		void Create();

		/// <summary>Demands database to be deleted and waits for result of the operation.</summary>
		void Delete();

		/// <summary>Demands database status information and waits for result of the operation.</summary>
		DatabaseInfo RequestInfo();
	}
}