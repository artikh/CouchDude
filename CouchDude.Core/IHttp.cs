using System;
using System.IO;
using System.Net;

namespace CouchDude.Core
{
	/// <summary>Abstraction over HTTP access.</summary>
	public interface IHttp
	{
		/// <summary>Returns server response to request to provided URL.</summary>
		TextReader RequestAndOpenTextReader(Uri uri, string method, TextReader content = null);

		/// <summary>Requests server returning only headers.</summary>
		WebHeaderCollection RequestAndGetHeaders(Uri uri, string method, TextReader content = null);

		/// <summary>Requests server without reading response body.</summary>
		void Request(Uri uri, string method, TextReader content = null);
	}
}