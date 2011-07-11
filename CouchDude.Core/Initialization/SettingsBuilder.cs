using System;
using System.Collections.Generic;
using System.Reflection;
using CouchDude.Core.Configuration;

namespace CouchDude.Core.Initialization
{
	/// <summary>Fluent interface for building of Settings instance.</summary>
	public class SettingsBuilder
	{
		private readonly Settings settings = new Settings();
		private readonly ISet<Assembly> assembliesToScan = new HashSet<Assembly>();

		/// <summary>Sets CouchDB server URI.</summary>
		public SettingsBuilder ServerUri(Uri serverUri)
		{
			settings.ServerUri = serverUri;
			return this;
		}

		/// <summary>Sets CouchDB server URI.</summary>
		public SettingsBuilder ServerUri(string serverUriString)
		{
			return ServerUri(new Uri(serverUriString, UriKind.Absolute));
		}

		/// <summary>Sets CouchDB database name.</summary>
		public SettingsBuilder DatabaseName(string dbName)
		{
			settings.DatabaseName = dbName;
			return this;
		}

		/// <summary>Forces CouchDude to request services (in particular conventions) it needs from
		/// given <param name="serviceProvider"/> instance.</summary>
		public SettingsBuilder ConsumingServicesFrom(IServiceProvider serviceProvider)
		{
			settings.IdGenerator = serviceProvider.GetService<IIdGenerator>() ?? settings.IdGenerator;
			return this;
		}

		/// <summary>Starts process of mapping group of similar entites.</summary>
		public EntityListConfigBuilder MappingEntities()
		{
			return new EntityListConfigBuilder(this);
		}
		
	qeoq. dui
		/// <summary>Geterates settings object.</summary>
		public Settings CreateSettings()
		{

			if (settings.Incomplete)
				throw new ConfigurationException("You shoud provide database name and server URL before creating settings.");

			return settings;
		}

		/// <summary>Helper builder for <see cref="SettingsBuilder"/></summary>
		public class EntityListConfigBuilder
		{
			private readonly SettingsBuilder parent;
			private readonly ISet<Assembly> assembliesToScan = new HashSet<Assembly>();
			private readonly IList<Predicate<Type>> entityDetectionConditions = new List<Predicate<Type>>();  

			/// <constructor />
			public EntityListConfigBuilder(SettingsBuilder parent)
			{
				this.parent = parent;
			}

			/// <summary>Geterates settings object.</summary>
			public Settings CreateSettings()
			{
				Flush();
				return parent.CreateSettings();
			}

			/// <summary>Starts new process of mapping group of similar entites.</summary>
			public EntityListConfigBuilder MappingEntities()
			{
				Flush();
				return parent.MappingEntities();
			}

			/// <summary>Saves all collected configuration to parent.</summary>
			private void Flush()
			{
				foreach (var assemblyToScan in assembliesToScan)
					parent.assembliesToScan.Add(assemblyToScan);
			}

			/// <summary>Adds assembly where provided type is declared to entity classes scan list.</summary>
			public EntityListConfigBuilder FromAssemblyOf<T>()
			{
				assembliesToScan.Add(typeof (T).Assembly);
				return this;
			}

			/// <summary>Adds assembly of provided name to entity classes scan list.</summary>
			public EntityListConfigBuilder FromAssembly(string assemblyNameString)
			{
				assembliesToScan.Add(Assembly.Load(assemblyNameString));
				return this;
			}

			/// <summary>Adds base class restriction for found entity types.</summary>
			public EntityListConfigBuilder InheritedFrom<T>()
			{
				if (typeof(T).IsInterface)
					throw new ArgumentException(string.Format("Type {0} is an interface. Probably you should use Implements<T>() method insted.", typeof(T)));

				entityDetectionConditions.Add(type => type.IsSubclassOf(typeof(T)));
				return this;
			}
		}
	}
}