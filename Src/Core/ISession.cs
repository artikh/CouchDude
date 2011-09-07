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
using System.Threading.Tasks;

namespace CouchDude
{
	/// <summary>CouchDB session interface.</summary>
	public interface ISession: IDisposable
	{
		/// <summary>Attaches entities to the session, assigning it an identifier if needed.</summary>
		/// <remarks>No changes to database are made until <see cref="StartSavingChanges"/> is called.</remarks>
		void Save<TEntity>(params TEntity[] entities) where TEntity : class;

		/// <summary>Marks provided enties for deletion from CouchDB.</summary>
		/// <remarks>No changes to databas are made until <see cref="StartSavingChanges"/> is called.</remarks>
		void Delete<TEntity>(params TEntity[] entities) where TEntity : class;

		/// <summary>Attaches entity to the session, assigning it an identifier if needed.</summary>
		/// <remarks>No changes to database are made until <see cref="StartSavingChanges"/> is called.</remarks>
		void Save<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>Marks provided entity for deletion from CouchDB.</summary>
		/// <remarks>No changes to databas are made until <see cref="StartSavingChanges"/> is called.</remarks>
		void Delete<TEntity>(TEntity entity) where TEntity : class;

		/// <summary>Loads entity from CouchDB placing in to first level cache.</summary>
		Task<TEntity> Load<TEntity>(string entityId) where TEntity : class;

		/// <summary>Queries CouchDB view, returning  paged list of  ether documents or view data items.</summary>
		Task<IViewQueryResult<T>> Query<T>(ViewQuery query);

		/// <summary>Queries lucene-couchdb index.</summary>
		Task<ILuceneQueryResult<T>> QueryLucene<T>(LuceneQuery query);

		/// <summary>Synchronous session methods.</summary>
		ISynchronousSessionMethods Synchronously { get; }

		/// <summary>Exposes raw CouchDB APIs.</summary>
		ICouchApi RawApi { get; }

		/// <summary>Starts "save all changes to CouchDB" process returning immediately.</summary>
		Task StartSavingChanges();
		
		/// <summary>Saves all changes to CouchDB and waites for result.</summary>
		void SaveChanges();
	}
}