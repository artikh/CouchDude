using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CouchDude.Http;
using CouchDude.Utils;

namespace CouchDude.Api
{
	internal class CouchApi: ICouchApi
	{
		private readonly ISynchronousCouchApi synchronousCouchApi;
		private readonly IHttpClient httpClient;
		private readonly Uri serverUri;
		
		/// <constructor />
		public CouchApi(IHttpClient httpClient, Uri serverUri)
		{
			this.httpClient = httpClient;
			this.serverUri = serverUri;
			synchronousCouchApi = new SynchronousCouchApi(this);
		}

		public IDatabaseApi Db(string databaseName)
		{
			if(string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentNullException("databaseName");
			CheckIf.DatabaseNameIsOk(databaseName, "databaseName");

			return new DatabaseApi(httpClient, serverUri, databaseName);
		}

		public Task<ICollection<string>> RequestAllDbNames() { throw new NotImplementedException(); }

		public ISynchronousCouchApi Synchronously { get { return synchronousCouchApi; } }
	}
}