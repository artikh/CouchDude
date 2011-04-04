using System.IO;
using System.Net;

namespace CouchDude.Core.HttpClient
{
	/// <summary>Describes HTTP response.</summary>
	public class HttpResponse
	{
		/// <summary>HTTP response status code.</summary>
		public HttpStatusCode Status { get; set; }

		/// <summary>HTTP headers including authentication and cookies.</summary>
		public WebHeaderCollection Headers { get; set; }

		/// <summary>HTTP response body.</summary>
		public TextReader Body { get; set; }

		/// <summary>Determines if request was succsessful.</summary>
		public bool IsOk { get { return (int) Status >= 200 && (int) Status < 300; } }
	}
}