#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CouchDude.Configuration.Builders
{
	/// <summary>Fluent interface for building of Settings instance.</summary>
	public class SettingsBuilder
	{
		private readonly Settings settings = new Settings();
		private readonly IDictionary<Assembly, ICollection<ScanDescriptor>> scanTasks = 
			new Dictionary<Assembly,ICollection<ScanDescriptor>>();

		private readonly Predicate<Type> globalEntityPredicate;
		
		/// <constructor />
		public SettingsBuilder() : this(null) { }
		
		/// <constructor />
		public SettingsBuilder(Predicate<Type> globalEntityPredicate)
		{
			this.globalEntityPredicate = globalEntityPredicate ?? DefaultGlobalEntityPredicate;
		}

		private static bool DefaultGlobalEntityPredicate(Type t)
		{
			return t.IsClass && !t.IsAbstract && !t.IsNotPublic && !t.IsNested;
		}

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

		/// <summary>Sets default database name to use.</summary>
		public SettingsBuilder DefaultDatabaseName(string dbName)
		{
			settings.DefaultDatabaseName = dbName;
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

		/// <summary>Starts process of mapping single entity.</summary>
		public SingleEntityConfigBuilder MappingEntitiy<T>()
		{
			return MappingEntitiy(typeof(T));
		}

		/// <summary>Starts process of mapping single entity.</summary>
		public SingleEntityConfigBuilder MappingEntitiy(Type entityType)
		{
			return new SingleEntityConfigBuilder(entityType, this);
		}

		internal void RegisterScanDescriptor(Assembly assembly, ScanDescriptor scanDescriptor)
		{
			ICollection<ScanDescriptor> scanDescriptors;
			if(!scanTasks.TryGetValue(assembly, out scanDescriptors))
				scanTasks.Add(assembly, scanDescriptors = new List<ScanDescriptor>());

			scanDescriptors.Add(scanDescriptor);
		}
		
		/// <summary>Geterates settings object.</summary>
		public Settings CreateSettings()
		{
			var entitySettingsToRegister =
				from pair in scanTasks
				let assembly = pair.Key
				let descriptors = pair.Value
				from type in assembly.GetTypes()
				where globalEntityPredicate(type)
				from descriptor in descriptors
				where descriptor.Predicate(type)
				let configSettings = descriptor.EntityConfigSettings
				where configSettings != null
				group configSettings by type;

			foreach (var configSettingsGroup in entitySettingsToRegister)
			{
				var configSettingsList = configSettingsGroup.Reverse().ToArray();
				var configSettings = new EntityConfigSettings();
				foreach (var setting in configSettingsList)
					configSettings.Merge(setting);
				var entityConfig = configSettings.Create(configSettingsGroup.Key);
				if (entityConfig != null)
					settings.Register(entityConfig);
			}

			if (settings.Incomplete)
				throw new ConfigurationException("You shoud provide database name and server URL before creating settings.");

			return settings;
		}
	}
}