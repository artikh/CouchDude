using System;
using System.Collections.Generic;
using CouchDude.Core.Configuration;

namespace CouchDude.Core.Initialization
{
	/// <summary>Fluent interface for building of ISessionFactory instance.</summary>
	public class SessionFactoryBuilder
	{
		internal readonly Settings Settings = new Settings();
		internal readonly Queue<Action> PreSettingsGenerationActions = new Queue<Action>();

		/// <summary>Sets CouchDB server URI.</summary>
		public SessionFactoryBuilder ServerUri(Uri serverUri)
		{
			Settings.ServerUri = serverUri;
			return this;
		}

		/// <summary>Sets CouchDB server URI.</summary>
		public SessionFactoryBuilder ServerUri(string serverUriString)
		{
			return ServerUri(new Uri(serverUriString, UriKind.Absolute));
		}

		/// <summary>Sets CouchDB database name.</summary>
		public SessionFactoryBuilder DatabaseName(string dbName)
		{
			Settings.DatabaseName = dbName;
			return this;
		}

		/// <summary>Forces CouchDude to request services (in particular conventions) it needs from
		/// given <param name="serviceProvider"/> instance.</summary>
		public SessionFactoryBuilder ConsumingServicesFrom(IServiceProvider serviceProvider)
		{
			Settings.IdGenerator = serviceProvider.GetService<IIdGenerator>() ?? Settings.IdGenerator;
			return this;
		}
		
		/// <summary>Geterates settings object.</summary>
		public Settings CreateSettings()
		{

			if (Settings.Incomplete)
				throw new ConfigurationException("You shoud provide database name and server URL before creating settings.");

			return Settings;
		}
	}
}