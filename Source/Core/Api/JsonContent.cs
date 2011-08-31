using System.Net.Http;
using System.Text;

namespace CouchDude.Api
{
	internal class JsonContent: StringContent
	{
		private const string JsonMediaType = "application/json";

		/// <constructor />
		public JsonContent(string jsonString): base(jsonString, Encoding.UTF8, JsonMediaType) { }

		/// <constructor />
		public JsonContent(IDocument document): this(document.ToString()) { }
	}
}