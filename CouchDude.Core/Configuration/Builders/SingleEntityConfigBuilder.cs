using System;
using CouchDude.Core.Configuration;

namespace CouchDude.Core.Configuration.Builders
{
	/// <summary>Helper builder for <see cref="SettingsBuilder"/></summary>
	public class SingleEntityConfigBuilder: SingleEntityConfigBuilder<SingleEntityConfigBuilder>
	{
		/// <constructor />
		public SingleEntityConfigBuilder(Type entityType, SettingsBuilder parent) : base(entityType, parent) { }
	}

	/// <summary>Helper builder for <see cref="SettingsBuilder"/></summary>
	public class SingleEntityConfigBuilder<TSelf> : EntityConfigBuilder<TSelf> where TSelf: SingleEntityConfigBuilder<TSelf>
	{
		/// <constructor />
		public SingleEntityConfigBuilder(Type entityType, SettingsBuilder parent): base(parent)
		{
			Predicates.Add(t => t == entityType);
			AssembliesToScan.Add(entityType.Assembly);
		}

		/// <summary>Registers custom <see cref="IEntityConfig"/> factory and returs user to parent builder.</summary>
		/// <remarks>This discards all previous <see cref="IEntityConfig"/> settings.</remarks>
		public SettingsBuilder WithCustomConfig(Func<Type, IEntityConfig> configFactory)
		{
			RegisterNewScanDescriptor(configFactory);
			return Parent;
		}
	}
}