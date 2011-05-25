using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace CouchDude.Core.HttpClient
{
	/// <summary><see cref="IHttpClient"/> implementation using 
	/// <see cref="HttpWebRequest"/>/<see cref="HttpWebResponse"/>.</summary>
	internal class HttpClientImpl: IHttpClient
	{
		private static readonly UTF8Encoding RequestEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

		/// <summary>Makes HTTP request and waits for result.</summary>
		public HttpResponse MakeRequest(HttpRequest request)
		{
			try
			{
				var requestTask = StartRequest(request);
				requestTask.Wait();
				return requestTask.Result;
			}
			catch (AggregateException e)
			{
				if (e.InnerExceptions.Count == 1)
					throw e.InnerExceptions[0];
				throw;
			}
		}

		/// <summary>Starts HTTP request and returs task.</summary>
		public Task<HttpResponse> StartRequest(HttpRequest requestDescriptor)
		{
			var httpWebRequest = CreateHttpWebRequest(requestDescriptor);

			return Task.Factory
				.FromAsync(
					httpWebRequest.BeginGetResponse,
					asyncResult => EndResponse(httpWebRequest, asyncResult),
					state: null)
				.ContinueWith(
					previousTask => {
						var requestBody = requestDescriptor.Body;
						if (requestBody != null)
							requestBody.Dispose();
						return previousTask.Result;
					})
				.ContinueWith(previousTask => CreateResponseDescripter(previousTask.Result));
		}

		private static HttpWebResponse EndResponse(WebRequest httpWebRequest, IAsyncResult asyncResult)
		{
			try
			{
				return (HttpWebResponse)httpWebRequest.EndGetResponse(asyncResult);
			}
			catch(WebException webException)
			{
				if (webException.Status == WebExceptionStatus.ProtocolError
				    && webException.Response != null)
					return (HttpWebResponse) webException.Response;
				throw;
			}
		}

		private static HttpResponse CreateResponseDescripter(HttpWebResponse httpWebResponse)
		{
			StreamReader bodyTextReader = null;
			var stream = httpWebResponse.GetResponseStream();
			if(stream != null)
			{
				var encoding = GetEncoding(httpWebResponse.CharacterSet, httpWebResponse.ContentEncoding);
				bodyTextReader = new StreamReader(stream, encoding);
			}

			return new HttpResponse {
				Status = httpWebResponse.StatusCode,
				Headers = httpWebResponse.Headers,
				Body = bodyTextReader
			};
		}

		private static HttpWebRequest CreateHttpWebRequest(HttpRequest requestDescriptor)
		{
			var httpWebRequest = (HttpWebRequest) WebRequest.Create(requestDescriptor.Uri);
			httpWebRequest.AllowAutoRedirect = false;
			httpWebRequest.AuthenticationLevel = AuthenticationLevel.None;
			httpWebRequest.Method = requestDescriptor.Method;
			if (requestDescriptor.Headers != null) 
				httpWebRequest.Headers = requestDescriptor.Headers;

			WriteRequestBody(httpWebRequest, requestDescriptor.Body);

			return httpWebRequest;
		}

		private static void WriteRequestBody(HttpWebRequest httpWebRequest, TextReader requestBody) {
			var buffer = new char[512];
			int charRead;
			if (requestBody != null)
				using (requestBody)
				using (var stream = httpWebRequest.GetRequestStream())
				using (var writer = new StreamWriter(stream, RequestEncoding))
					while ((charRead = requestBody.Read(buffer, index: 0, count: buffer.Length)) > 0)
						writer.Write(buffer, index: 0, count: charRead);
		}

		/// <summary>Guesses encoding of the response's body.</summary>
		internal static Encoding GetEncoding(string characterSet, string contentEncoding)
		{
			if (!string.IsNullOrWhiteSpace(characterSet))
			{
				var charSetEncoding = TryGetEncoding(characterSet);
				if (charSetEncoding != null)
					return charSetEncoding;
			}

			if (!string.IsNullOrWhiteSpace(contentEncoding))
			{
				var charSetEncoding = TryGetEncoding(contentEncoding);
				if (charSetEncoding != null)
					return charSetEncoding;
			}

			return Encoding.UTF8;
		}

		private static Encoding TryGetEncoding(string encodingName)
		{
			try
			{
				return Encoding.GetEncoding(encodingName);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}