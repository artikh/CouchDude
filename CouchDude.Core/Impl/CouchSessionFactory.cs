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
using CouchDude.Core.Api;
using CouchDude.Core.Http;

namespace CouchDude.Core.Impl
{
	/// <summary>Session factory implementation.</summary>
	public class CouchSessionFactory: ISessionFactory
	{
		private readonly ICouchApi couchApi;
		private readonly Settings settings;

		/// <constructor />
		public CouchSessionFactory(Settings settings, ICouchApi couchApi = null)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			this.settings = settings;
			this.couchApi = couchApi 
				?? new CouchApi(new HttpClientImpl(), settings.ServerUri, settings.DatabaseName);
		}

		/// <inheritdoc/>
		public ISession CreateSession()
		{
			return new CouchSession(settings, couchApi);
		}
	}
}