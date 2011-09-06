using System.Collections.Generic;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class SynchronousCouchApi : ISynchronousCouchApi
	{
		private readonly ICouchApi couchApi;

		/// <constructor />
		public SynchronousCouchApi(ICouchApi couchApi) { this.couchApi = couchApi; }

		public ICollection<string> RequestAllDbNames() { return couchApi.RequestAllDbNames().WaitForResult(); }
	}
}