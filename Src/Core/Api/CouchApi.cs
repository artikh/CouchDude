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
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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
		private readonly IReplicatorApi replicatorApi;

		/// <constructor />
		public CouchApi(IHttpClient httpClient, Uri serverUri)
		{
			this.httpClient = httpClient;
			this.serverUri = serverUri;
			synchronousCouchApi = new SynchronousCouchApi(this);
			replicatorApi = new ReplicatorApi(this);
		}

		public IReplicatorApi Replicator { get { return replicatorApi; } }

		public IDatabaseApi Db(string databaseName)
		{
			if(String.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException("databaseName");
			CheckIf.DatabaseNameIsOk(databaseName, "databaseName");

			return new DatabaseApi(httpClient, serverUri, databaseName);
		}

		public Task<ICollection<string>> RequestAllDbNames()
		{
			var allDbsUri = new Uri(serverUri, "_all_dbs");
			var request = new HttpRequestMessage(HttpMethod.Get, allDbsUri);
			return StartRequest(request, httpClient).ContinueWith(
				rt => {
					var response = rt.Result;

					if (!response.IsSuccessStatusCode)
						new CouchError(response).ThrowCouchCommunicationException();
					using (var responseReader = response.Content.GetTextReader())
					{
						var responseJson = new JsonFragment(responseReader);
						var dbs = responseJson.TryDeserialize(typeof(string[])) as ICollection<string>;
						if(dbs == null)
							throw new CouchCommunicationException("Unknown data recived from CouchDB: {0}", responseJson);
						return dbs;
					}
				});
		}

		public ISynchronousCouchApi Synchronously { get { return synchronousCouchApi; } }

		public IReplicatorApi ReplicatorApi { get { return replicatorApi; } }

		internal static Task<HttpResponseMessage> StartRequest(HttpRequestMessage request, IHttpClient htmlClient)
		{
			return htmlClient
				.StartRequest(request)
				.ContinueWith(
					t => {
						if (t.IsFaulted && t.Exception != null)
						{
							var innerExceptions = t.Exception.InnerExceptions;
							

							var newInnerExceptions = new Exception[innerExceptions.Count];
							for (var i = 0; i < innerExceptions.Count; i++)
							{
								var e = innerExceptions[i];
								newInnerExceptions[i] = 
									e is WebException || e is SocketException || e is HttpException
										? new CouchCommunicationException(e)
										: e;
							}
							throw new AggregateException(t.Exception.Message, newInnerExceptions);
						}
						return t.Result;
					});
		}
	}
}