using System;

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
			this.couchApi = couchProxy ?? new CouchApi(new Http(), settings.BaseUri);
		}

		/// <inheritdoc/>
		public ISession CreateSession()
		{
			return new CouchSession(settings, couchApi);
		}
	}
}