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
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class CouchApi: HttpClient, ICouchApi
	{
		private readonly ISynchronousCouchApi synchronousCouchApi;
		private readonly UriConstructor uriConstructor;
		private readonly IReplicatorApi replicatorApi;

		/// <constructor />
		public CouchApi(Uri serverUri): this(serverUri, null) { }

		/// <constructor />
		public CouchApi(Uri serverUri, HttpMessageHandler handler): base(handler)
		{
			uriConstructor = new UriConstructor(serverUri);
			synchronousCouchApi = new SynchronousCouchApi(this);
			replicatorApi = new ReplicatorApi(this);
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
			var request = new HttpRequestMessage(HttpMethod.Get, uriConstructor.AllDbUri);
			return Request(request).ContinueWith(
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

		internal Task<HttpResponseMessage> Request(HttpRequestMessage request)
		{
			return SendAsync(request)
				.ContinueWith(
					t => {
						if (!t.IsFaulted)
							return t.Result;

						// ReSharper disable PossibleNullReferenceException
						var innerExceptions = t.Exception.InnerExceptions;
						// ReSharper restore PossibleNullReferenceException

						var newInnerExceptions = new Exception[innerExceptions.Count];
						for (var i = 0; i < innerExceptions.Count; i++)
						{
							var e = innerExceptions[i];
							newInnerExceptions[i] =
								e is WebException || e is SocketException || e is HttpRequestException
									? new CouchCommunicationException(e)
									: e;
						}
						throw new AggregateException(t.Exception.Message, newInnerExceptions);
					}
				);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}