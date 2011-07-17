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

using CouchDude.Core.Impl;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core.Api
{
	/// <summary>Represents lower-level CouchDB API: in-between HTTP and ISession.</summary>
	public interface ICouchApi
	{
		/// <summary>Requests CouchDB for document.</summary>
		JObject GetDocumentFromDbById(string docId);

		/// <summary>Saves new document in CouchDB.</summary>
		JObject SaveDocumentToDb(string docId, JObject document);

		/// <summary>Updates document in CouchDB.</summary>
		JObject UpdateDocumentInDb(string docId, JObject document);

		/// <summary>Retrives current document revision from database. <c>null</c> returned if
		/// there is no such document in database.</summary>
		string GetLastestDocumentRevision(string docId);

		/// <summary>Deletes document of provided <param name="docId"/> if it's revision
		/// is equal to provided <param name="revision"/>.</summary>
		JObject DeleteDocument(string docId, string revision);

		/// <summary>Queries CouchDB view.</summary>
		ViewResult Query(ViewQuery query);

		/// <summary>Queries CouchDB view.</summary>
		LuceneResult FulltextQuery(LuceneQuery query);
	}
}