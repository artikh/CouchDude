using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CouchDude.Core.HttpClient;

namespace CouchDude.Tests
{
	public class HttpClientMock: IHttpClient
	{
		private readonly Exception exception;
		private readonly HttpResponse response;
		
		public HttpClientMock(string responseText )
		{
			response = new HttpResponse {
				Status = HttpStatusCode.OK,
				Headers = new WebHeaderCollection(),
				Body = new StringReader(responseText)
			};
		}
		public HttpClientMock(HttpResponse response = null)
		{
			this.response = response ?? new HttpResponse {
				Status = HttpStatusCode.OK,
				Headers = new WebHeaderCollection(),
				Body = new StringReader(string.Empty)
			};
		}

		public HttpClientMock(Exception exception)
		{
			this.exception = exception;
		}

		public HttpRequest Request { get; private set; }

		public Task<HttpResponse> StartRequest(HttpRequest request)
		{
			if (exception != null)
				throw exception;
			Request = request;
			return Task.Factory.StartNew(() => response);
		}

		public HttpResponse MakeRequest(HttpRequest request)
		{
			if (exception != null)
				throw exception;
			Request = request;
			return response;
		}
	}
}
