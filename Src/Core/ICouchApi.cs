#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov																					
																																					
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
	public interface ICouchApi
	{
		/// <summary>Requests CouchDB for document.</summary>
		Task<IDocument> RequestDocumentById(string docId);

		/// <summary>Saves new document to CouchDB.</summary>
		Task<DocumentInfo> SaveDocument(IDocument document);

		/// <summary>Retrives current document revision from database. </summary>
		/// <remarks><c>null</c> returned if there is no such document in database.</remarks>
		Task<string> RequestLastestDocumentRevision(string docId);

		/// <summary>Deletes document of provided <param name="documentId"/> if it's revision
		/// is equal to provided <param name="revision"/>.</summary>
		Task<DocumentInfo> DeleteDocument(string documentId, string revision);

		/// <summary>Queries CouchDB view.</summary>
		Task<IViewQueryResult> Query(ViewQuery query);

		/// <summary>Queries couchdb-lucene index.</summary>
		Task<ILuceneQueryResult> QueryLucene(LuceneQuery query);

		/// <summary>Creates, updates and deletes several documents as a whole. </summary>
		Task<IDictionary<string, DocumentInfo>> BulkUpdate(Action<IBulkUpdateBatch> updateCommandBuilder);

		/// <summary>Synchronous version of API.</summary>
		ISynchronousCouchApi Synchronously { get; }
	}
}