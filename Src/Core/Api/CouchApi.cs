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
using System.Collections.Generic;
using System.Threading.Tasks;
using CouchDude.Http;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class CouchApi: ICouchApi
	{
		private readonly ISynchronousCouchApi synchronousCouchApi;
		private readonly IHttpClient httpClient;
		private readonly Uri serverUri;
		
		/// <constructor />
		public CouchApi(IHttpClient httpClient, Uri serverUri)
		{
			this.httpClient = httpClient;
			this.serverUri = serverUri;
			synchronousCouchApi = new SynchronousCouchApi(this);
		}

		public IDatabaseApi Db(string databaseName)
		{
			if(string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException("databaseName");
			CheckIf.DatabaseNameIsOk(databaseName, "databaseName");

			return new DatabaseApi(httpClient, serverUri, databaseName);
		}

		public Task<ICollection<string>> RequestAllDbNames() { throw new NotImplementedException(); }

		public ISynchronousCouchApi Synchronously { get { return synchronousCouchApi; } }
	}
}