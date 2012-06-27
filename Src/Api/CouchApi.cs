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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class CouchApi: HttpClient, ICouchApi
	{
		private readonly ISynchronousCouchApi synchronousCouchApi;
		private readonly UriConstructor uriConstructor;
		private readonly IReplicatorApi replicatorApi;

		internal readonly CouchApiSettings Settings;
		
		/// <constructor />
		public CouchApi(CouchApiSettings settings, HttpMessageHandler messageHandler = null)
			: base(messageHandler ?? new HttpClientHandler())
		{
			Settings = settings;

			uriConstructor = new UriConstructor(settings.ServerUri);
			synchronousCouchApi = new SynchronousCouchApi(this);
			replicatorApi = new ReplicatorApi(this);

			BaseAddress = settings.ServerUri;
			DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType.Json));
			DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
			DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
			if (settings.Credentials != null)
				DefaultRequestHeaders.Authorization = settings.Credentials.ToAuthenticationHeader();
		}

		public IReplicatorApi Replicator { get { return replicatorApi; } }

		public IDatabaseApi Db(string databaseName)
		{
			if(String.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException("databaseName");
			CheckIf.DatabaseNameIsOk(databaseName, "databaseName");

			return new DatabaseApi(this, uriConstructor.Db(databaseName));
		}

		public Task<ICollection<string>> RequestAllDbNames() 
		{
			using(SyncContext.SwitchToDefault())
				return RequestAllDbNamesInternal();
		}

		async Task<ICollection<string>> RequestAllDbNamesInternal()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, uriConstructor.AllDbUri);

			var response = await this.RequestCouchDb(request);

			if (!response.IsSuccessStatusCode)
				new CouchError(Settings.Serializer, response).ThrowCouchCommunicationException();

			using (var responseStream = await response.Content.ReadAsStreamAsync())
			using (var responseReader = new StreamReader(responseStream))
			{
				var dbs =
					(ICollection<string>)
						Settings.Serializer.Deserialize(typeof (string[]), responseReader, throwOnError: false);
				if (dbs == null)
				{
					var recivedString = await response.Content.ReadAsStringAsync();
					throw new CouchCommunicationException("Unexpected data recived from CouchDB: {0}", recivedString);
				}
				return dbs;
			}
		}

		public ISynchronousCouchApi Synchronously { get { return synchronousCouchApi; } }

		public IReplicatorApi ReplicatorApi { get { return replicatorApi; } }

	}
}