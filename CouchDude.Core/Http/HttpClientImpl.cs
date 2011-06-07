using System;
using System.Net;
using System.Reflection;
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