using System;

namespace CouchDude.Core.Configuration.Builders
{
	/// <summary>Base class for sub-builders.</summary>
	public abstract class SettingsBuilderDetour
	{
		/// <summary>Parent builder.</summary>
		protected readonly SettingsBuilder Parent;

		/// <constructor />
		protected SettingsBuilderDetour(SettingsBuilder parent)
		{
			Parent = parent;
		}

		/// <summary>Geterates settings object.</summary>
		public Settings CreateSettings()
		{
			Flush();
			return Parent.CreateSettings();
		}

		/// <summary>Starts new process of mapping group of similar entites.</summary>
		public EntityListConfigBuilder MappingEntities()
		{
			Flush();
			return Parent.MappingEntities();
		}

		/// <summary>Starts process of mapping single entity.</summary>
		public SingleEntityConfigBuilder MappingEntitiy<T>()
		{
			Flush();
			return Parent.MappingEntitiy<T>();
		}

		/// <summary>Starts process of mapping single entity.</summary>
		public SingleEntityConfigBuilder MappingEntitiy(Type entityType)
		{
			Flush();
			return Parent.MappingEntitiy(entityType);
		}

		/// <summary>Saves all collected configuration to parent.</summary>
		protected abstract void Flush();
	}
}