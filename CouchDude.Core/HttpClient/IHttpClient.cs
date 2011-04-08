using System.Threading.Tasks;

namespace CouchDude.Core.HttpClient
{
	/// <summary>HTTP client abstracted interface.</summary>
	internal interface IHttpClient
	{
		/// <summary>Starts HTTP request and returs task.</summary>
		Task<HttpResponse> StartRequest(HttpRequest request);

		/// <summary>Makes HTTP request and waits for result.</summary>
		HttpResponse MakeRequest(HttpRequest request);
	}
}