﻿#region Licence Info 
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

namespace CouchDude.Core
{
	/// <summary>CouchDB session interface.</summary>
	public interface ISession: IDisposable
	{
		/// <summary>Attaches entity to the session and saves it to
		/// CouchDB.</summary>
		DocumentInfo Save<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>Loads entity from CouchDB placing in to first level cache.</summary>
		TEntity Load<TEntity>(string entityId) where TEntity : class;

		/// <summary>Synchronises all changes to CouchDB.</summary>
		void SaveChanges();

		/// <summary>Deletes provided entity form CouchDB.</summary>
		DocumentInfo Delete<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>Queries CouchDB view returning ether paged list of documents or view data items.</summary>
		IPagedList<T> Query<T>(ViewQuery<T> query);

		/// <summary>Queries LuceneCouchDB</summary>
		IPagedList<T> FulltextQuery<T>(LuceneQuery<T> query) where T : class;
	}
}