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
using CouchDude.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDude.Tests
{
	internal class HttpClientMock: IHttpClient
	{
		private readonly Exception exception;
		private readonly HttpResponseMessage response;

		public HttpClientMock(string responseText): this(HttpStatusCode.OK, responseText) { }

		public HttpClientMock(HttpStatusCode code, string responseText)
		{
			response =
				new HttpResponseMessage
					{
						StatusCode = code,
						Content = new StringContent(responseText)
					};
		}

		public HttpClientMock(HttpResponseMessage response = null)
		{
			this.response = response ?? new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent("\"ok\":true")};
		}

		public HttpClientMock(Exception exception)
		{
			this.exception = exception;
		}

		public HttpRequestMessage Request { get; private set; }

		public Task<HttpResponseMessage> StartRequest(HttpRequestMessage requestMessage)
		{
			Request = requestMessage;
			return Task.Factory.StartNew(
				() => {
					if (exception != null)
						throw exception;
					return response;
				});
		}

		public HttpResponseMessage MakeRequest(HttpRequestMessage requestMessage)
		{
			if (exception != null)
				throw exception;
			Request = requestMessage;
			return response;
		}
	}
}
