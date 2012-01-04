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

namespace CouchDude.Api
{
	/// <summary>Wraps weak reference to database API.</summary>
	public struct DatabaseApiReference
	{
		/// <summary>Default value of the refernce.</summary>
		public static readonly DatabaseApiReference Empty = default(DatabaseApiReference);

		readonly WeakReference dbApiWeakReference;

		/// <constructor />
		public DatabaseApiReference(IDatabaseApi dbApi = null)
		{
			dbApiWeakReference = dbApi == null? null: new WeakReference(dbApi, trackResurrection: false);
		}

		/// <summary>Returns <see cref="IDatabaseApi"/> </summary>
		public IDatabaseApi GetOrThrowIfUnavaliable(Func<string> operation)
		{
			if (dbApiWeakReference != null)
			{
				var dbApi = dbApiWeakReference.Target as IDatabaseApi;
				if (dbApi != null) 
					return dbApi;
			}
			throw new LazyLoadingException("Unable to {0} because IDatabaseApi is unavaliable", operation());
		}
	}
}
