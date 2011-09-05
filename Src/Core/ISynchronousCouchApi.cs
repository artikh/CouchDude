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
	/// <summary>Represents synchronous version of low-level CouchDB API.</summary>
	public interface ISynchronousCouchApi
	{
		/// <summary>Requests <paramref name="databaseName"/> for document and waits for an answer.</summary>
		IDocument RequestDocumentById(string databaseName, string docId);

		/// <summary>Saves new document to <paramref name="databaseName"/> and waits for the result of the operation.</summary>
		DocumentInfo SaveDocument(string databaseName, IDocument document);

		/// <summary>Retrives current document revision from <paramref name="databaseName"/> and waits for the result of the operation. </summary>
		/// <remarks><c>null</c> returned if there is no such document in database.</remarks>
		string RequestLastestDocumentRevision(string databaseName, string docId);

		/// <summary>Deletes document of provided <paramref name="docId"/> if it's revision
		/// is equal to provided <paramref name="revision"/> from <paramref name="databaseName"/> and waits for the result of the operation.</summary>
		DocumentInfo DeleteDocument(string databaseName, string docId, string revision);

		/// <summary>Creates, updates and deletes several documents as a whole. </summary>
		IDictionary<string, DocumentInfo> BulkUpdate(string databaseName, Action<IBulkUpdateBatch> updateCommandBuilder);

		/// <summary>Queries CouchDB view defined in <paramref name="databaseName"/> and waits for the result of the operation.</summary>
		IViewQueryResult Query(string databaseName, ViewQuery query);

		/// <summary>Queries couchdb-lucene index defined in <paramref name="databaseName"/>.</summary>
		ILuceneQueryResult QueryLucene(string databaseName, LuceneQuery query);
	}
}