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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace CouchDude.Http
{
	/// <summary><see cref="IHttpClient"/> implementation using 
	/// <see cref="HttpWebRequest"/>/<see cref="HttpWebResponse"/>.</summary>
	internal class HttpClientImpl: IHttpClient, IDisposable
	{
		public ThreadLocal<HttpClient> HttpClient = new ThreadLocal<HttpClient>(() => new HttpClient()); 

		/// <summary>Makes HTTP request and waits for result.</summary>
		public HttpResponseMessage MakeRequest(HttpRequestMessage requestMessage)
		{
			return HttpClient.Value.Send(requestMessage);
		}

		/// <summary>Starts HTTP request and returs task.</summary>
		public Task<HttpResponseMessage> StartRequest(HttpRequestMessage requestMessage)
		{
			/*
			 * Async version sadly throws Exception on none-null return codes at the time of witing. Should revise later.
			 * return HttpClient.Value.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
			 */
			return Task.Factory.StartNew(() => HttpClient.Value.Send(requestMessage));
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(finalizing: false);
			GC.SuppressFinalize(this);
		}

		~HttpClientImpl()
		{
			Dispose(true);
		}

		private void Dispose(bool finalizing)
		{
			if(!finalizing)
				HttpClient.Dispose();
		}
	}
}