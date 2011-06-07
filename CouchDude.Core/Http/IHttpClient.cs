using System.Threading.Tasks;
using System.Net.Http;

namespace CouchDude.Core.Http
{
	/// <summary>HTTP client abstracted interface.</summary>
	internal interface IHttpClient
	{
		/// <summary>Starts HTTP request and returs task.</summary>
		Task<HttpResponseMessage> StartRequest(HttpRequestMessage requestMessage);

		/// <summary>Makes HTTP request and waits for result.</summary>
		HttpResponseMessage MakeRequest(HttpRequestMessage requestMessage);
	}
}