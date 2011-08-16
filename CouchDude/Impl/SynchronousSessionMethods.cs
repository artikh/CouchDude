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

using CouchDude.Utils;

namespace CouchDude.Impl
{
	/// <summary>Synchronous query methods for <see cref="ISession"/>.</summary>
	public class SynchronousSessionMethods: ISynchronousSessionMethods
	{
		private readonly ISession session;

		/// <constructor />
		public SynchronousSessionMethods(ISession session)
		{
			this.session = session;
		}

		/// <inheritdoc/>
		public IPagedList<T> Query<T>(ViewQuery<T> query)
		{
			return session.Query(query).WaitForResult();
		}

		/// <inheritdoc/>
		public IPagedList<T> FulltextQuery<T>(LuceneQuery<T> query) where T : class
		{
			return session.FulltextQuery(query).WaitForResult();
		}

		/// <inheritdoc/>
		public TEntity Load<TEntity>(string entityId) where TEntity : class
		{
			return session.Load<TEntity>(entityId).WaitForResult();
		}
	}
}