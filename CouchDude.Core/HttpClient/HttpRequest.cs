using System;
using System.IO;
using System.Net;

namespace CouchDude.Core.HttpClient
{
	/// <summary>Describes HTTP request.</summary>
	public class HttpRequest
	{
		/// <constructor />
		public HttpRequest(Uri uri, string method, WebHeaderCollection headers = null, TextReader body = null)
		{
			Uri = uri;
			Method = method;
			Headers = headers;
			Body = body;
		}

		/// <summary>Address to make request to.</summary>
		public Uri Uri { get; private set; }

		/// <summary>HTTP method.</summary>
		public string Method { get; private set; }

		/// <summary>HTTP headers including authentication and cookies.</summary>
		public WebHeaderCollection Headers { get; private set; }

		/// <summary>HTTP request body.</summary>
		public TextReader Body { get; private set; }
	}
}
