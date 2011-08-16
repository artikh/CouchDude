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

namespace CouchDude.Core
{
	/// <summary>CouchDB session synchronous methods interface.</summary>
	public interface ISynchronousSessionMethods
	{
		/// <summary>Queries CouchDB view, returning  paged list of  ether documents or view data items waiting for result.</summary>
		IPagedList<T> Query<T>(ViewQuery<T> query);

		/// <summary>Queries lucene-couchdb index waiting for the result.</summary>
		IPagedList<T> FulltextQuery<T>(LuceneQuery<T> query) where T : class;

		/// <summary>Loads entity from CouchDB placing in to first level cache.</summary>
		TEntity Load<TEntity>(string entityId) where TEntity : class;
	}
}