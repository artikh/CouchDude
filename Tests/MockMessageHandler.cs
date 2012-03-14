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
using System.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CouchDude.Tests
{
	internal class MockMessageHandler : HttpMessageHandler
	{
		private readonly Exception exception;
		private readonly HttpResponseMessage response;
		public HttpRequestMessage Request { get; private set; }
		public string RequestBodyString { get; private set; }

		public MockMessageHandler(JsonValue responseJson) 
			: this(HttpStatusCode.OK, responseJson.ToString(), "application/json") { }

		public MockMessageHandler(HttpStatusCode code, JsonValue responseJson) 
			: this(code, responseJson.ToString(), "application/json") { }

		public MockMessageHandler(string responseText, string contentType) 
			: this(HttpStatusCode.OK, responseText, contentType) { }

		public MockMessageHandler(HttpStatusCode code, string responseText, string contentType)
			: this(new HttpResponseMessage {
					StatusCode = code, Content = new StringContent(responseText, Encoding.UTF8, contentType)
			}) { }

		public MockMessageHandler(HttpResponseMessage response = null)
		{
			this.response = response
				??
				new HttpResponseMessage
				{StatusCode = HttpStatusCode.OK, Content = new StringContent("\"ok\":true")};
		}

		public MockMessageHandler(Exception exception)
		{
			this.exception = exception;
		}
		
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			Request = request;
			RequestBodyString = Request.Content != null? Request.Content.ReadAsStringAsync().Result: null;
			return Task.Factory.StartNew(() => SendInternal());
		}

		private HttpResponseMessage SendInternal()
		{
			if (exception != null)
				throw exception;
			return response;
		}
	}
}

