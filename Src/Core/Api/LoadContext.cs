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

namespace CouchDude.Api
{
	internal struct DatabaseApiReference
	{
		public static readonly DatabaseApiReference Empty = new DatabaseApiReference(null);		

		private readonly WeakReference dbApiWeakReference;

		/// <constructor />
		public DatabaseApiReference(IDatabaseApi dbApi = null)
		{
			dbApiWeakReference = dbApi == null? null: new WeakReference(dbApi, trackResurrection: false);
		}

		public IDatabaseApi GetOrThrowIfUnavaliable(Func<string> operation)
		{
			var dbApi = dbApiWeakReference.Target as IDatabaseApi;
			if (dbApi == null)
				throw new LazyLoadingException("Unable to {0} because IDatabaseApi is unavaliable", operation());
			return dbApi;
		}
	}
}
