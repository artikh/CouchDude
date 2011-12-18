using System.Json;
using System.Net.Http;
using System.Text;

namespace CouchDude.Utils
{
	/// <summary>JSON HTTP request body.</summary>
	public class JsonContent: StringContent
	{
		/// <constructor />
		public JsonContent(JsonValue json) 
			: base(json.ToString(JsonSaveOptions.None), Encoding.UTF8, "application/json") { }
	}
}
