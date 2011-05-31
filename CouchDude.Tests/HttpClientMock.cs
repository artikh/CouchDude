using System;
using System.Net; 
using CouchDude.Core.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace CouchDude.Tests
{
	internal class HttpClientMock: IHttpClient
	{
		private readonly Exception exception;
		private readonly HttpResponseMessage response;
		
		public HttpClientMock(string responseText)
		{
			response =
				new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent(responseText)
					};
		}

		public HttpClientMock(HttpResponseMessage response = null)
		{
			this.response = response ?? new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent(string.Empty)};
		}

		public HttpClientMock(Exception exception)
		{
			this.exception = exception;
		}

		public HttpRequestMessage Request { get; private set; }

		public Task<HttpResponseMessage> StartRequest(HttpRequestMessage requestMessage)
		{
			if (exception != null)
				throw exception;
			Request = requestMessage;
			return Task.Factory.StartNew(() => response);
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
