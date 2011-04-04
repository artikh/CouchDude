using System;
using CouchDude.Core.HttpClient;

namespace CouchDude.Core.Implementation
{
	/// <summary>Session factory implementation.</summary>
	public class CouchSessionFactory: ISessionFactory
	{
		private readonly ICouchApi couchApi;
		private readonly Settings settings;

		/// <constructor />
		public CouchSessionFactory(Settings settings, ICouchApi couchProxy = null)
		{
			if (settings == null) throw new ArgumentNullException("settings");

			this.settings = settings;
			couchApi = couchProxy 
				?? new CouchApi(new HttpClientImpl(), settings.ServerUri, settings.DatabaseName);
		}

		/// <inheritdoc/>
		public ISession CreateSession()
		{
			return new CouchSession(settings, couchApi);
		}
	}
}