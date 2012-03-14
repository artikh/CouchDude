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

namespace CouchDude.Impl
{
	/*
	 * Design note. 
	 * This thing does not do mutch now, however I (A.T.) think it's neded for cross-session services later (not second level caching though).  
	 * Plus it adds familiarity for NH folks :)
	 */

	/// <summary>Session factory implementation.</summary>
	public class CouchSessionFactory: ISessionFactory
	{
		private readonly Settings settings;
		private readonly Func<Settings, ICouchApi> couchApiFactory;

		/// <constructor />
		internal CouchSessionFactory(Settings settings, Func<Settings, ICouchApi> couchApiFactory)
		{
			if (settings == null) throw new ArgumentNullException("settings");
			if (couchApiFactory == null) throw new ArgumentNullException("couchApiFactory");

			this.settings = settings;
			this.couchApiFactory = couchApiFactory;
		}

		/// <inheritdoc/>
		public ISession CreateSession()
		{
			return new CouchSession(settings, couchApiFactory(settings));
		}

		/// <inheritdoc/>
		public ISession CreateSession(string databaseName)
		{
			return new CouchSession(databaseName, settings, couchApiFactory(settings));
		}
	}
}