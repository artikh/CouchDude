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

using System.Net;
using System.Threading.Tasks;
using System.Net.Http;

namespace CouchDude.Core.Http
{
	/// <summary><see cref="IHttpClient"/> implementation using 
	/// <see cref="HttpWebRequest"/>/<see cref="HttpWebResponse"/>.</summary>
	internal class HttpClientImpl: IHttpClient
	{
		/// <summary>Makes HTTP request and waits for result.</summary>
		public HttpResponseMessage MakeRequest(HttpRequestMessage requestMessage)
		{
			using (var httpClient = new HttpClient())
				return httpClient.Send(requestMessage, HttpCompletionOption.ResponseContentRead);
		}

		/// <summary>Starts HTTP request and returs task.</summary>
		public Task<HttpResponseMessage> StartRequest(HttpRequestMessage requestMessage)
		{
			var httpClient = new HttpClient();
			var getResponseTask = httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead);
			getResponseTask.ContinueWith(sendTask => httpClient.Dispose());
			return getResponseTask;
		}
	}
}