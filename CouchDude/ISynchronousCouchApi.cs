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

namespace CouchDude
{
	/// <summary>Represents synchronous version of low-level CouchDB API.</summary>
	public interface ISynchronousCouchApi
	{
		/// <summary>Requests CouchDB for document and waits for an answer.</summary>
		IDocument RequestDocumentById(string docId);
		
		/// <summary>Saves new document to CouchDB and waits for the result of the operation.</summary>
		IJsonFragment SaveDocumentSync(IDocument document);

		/// <summary>Updates document in CouchDB and waits for the result of the operation.</summary>
		IJsonFragment UpdateDocument(IDocument document);

		/// <summary>Retrives current document revision from database and waits for the result of the operation. </summary>
		/// <remarks><c>null</c> returned if there is no such document in database.</remarks>
		string RequestLastestDocumentRevision(string docId);

		/// <summary>Deletes document of provided <param name="docId"/> if it's revision
		/// is equal to provided <param name="revision"/> and waits for the result of the operation.</summary>
		IJsonFragment DeleteDocument(string docId, string revision);

		/// <summary>Queries CouchDB view and waits for the result of the operation.</summary>
		IPagedList<ViewResultRow> Query(ViewQuery query);

		/// <summary>Queries CouchDB view.</summary>
		IPagedList<LuceneResultRow> QueryLucene(LuceneQuery query);
	}
}