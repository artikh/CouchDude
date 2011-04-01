using System;
using System.IO;
using System.Net;
using System.Text;

namespace CouchDude.Core.Implementation
{
	/// <summary>HTTP abstraction implementation.</summary>
	public class Http: IHttp
	{
		/// <inheritdoc/>
		public TextReader RequestAndOpenTextReader(Uri uri, string method, TextReader content = null)
		{
			var response = RequestInternal(uri, method, content);
			var encoding = GetEncoding(response);
			var stream = response.GetResponseStream();
			return stream != null 
				? (TextReader)new StreamReader(stream, encoding) 
				: new StringReader(string.Empty);
		}

		/// <inheritdoc/>
		public void Request(Uri uri, string method, TextReader content = null)
		{
			RequestInternal(uri, method, content);
		}

		/// <inheritdoc/>
		public WebHeaderCollection RequestAndGetHeaders(
			Uri uri, string method, TextReader content)
		{
			var response = RequestInternal(uri, method, content);
			var headers = response.Headers;
			return headers;
		}

		private static HttpWebResponse RequestInternal(
			Uri uri, string method, TextReader requestBodyReader = null)
		{
			var request = (HttpWebRequest) (WebRequest.Create(uri));
			request.Method = method;

			if(requestBodyReader != null)
				using (var requestStream = request.GetRequestStream())
				using (var requestWriter = 
					new StreamWriter(
						requestStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
				{
					int charRead;
					var buffer = new char[512];
					while ((charRead = requestBodyReader.ReadBlock(buffer, 0, buffer.Length)) > 0)
						requestWriter.Write(buffer, index: 0, count: charRead);
				}

			return (HttpWebResponse) request.GetResponse();
		}

		/// <summary>Guesses encoding of the response's body.</summary>
		internal static Encoding GetEncoding(HttpWebResponse response)
		{
			if (!string.IsNullOrWhiteSpace(response.CharacterSet))
			{
				var charSetEncoding = TryGetEncoding(response.CharacterSet);
				if (charSetEncoding != null)
					return charSetEncoding;
			}

			if (!string.IsNullOrWhiteSpace(response.ContentEncoding))
			{
				var charSetEncoding = TryGetEncoding(response.ContentEncoding);
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