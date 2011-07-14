using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using CouchDude.Core.Configuration;

namespace CouchDude.Core.Configuration.Builders
{
	/// <summary>Helps <see cref="SettingsBuilder"/> to build list of entity config class.</summary>
	public class EntityListConfigBuilder : EntityListConfigBuilder<EntityListConfigBuilder>
	{
		/// <constructor />
		public EntityListConfigBuilder(SettingsBuilder parent) : base(parent) {}
	}

	/// <summary>Helps <see cref="SettingsBuilder"/> to build list of entity config class.</summary>
	public class EntityListConfigBuilder<TSelf> : EntityConfigBuilder<TSelf> where TSelf: EntityListConfigBuilder<TSelf>
	{
		/// <constructor />
		public EntityListConfigBuilder(SettingsBuilder parent): base(parent) { }
			
		/// <summary>Adds assembly where provided type is declared to entity classes scan list.</summary>
		public TSelf FromAssemblyOf<T>()
		{
			AssembliesToScan.Add(typeof (T).Assembly);
			return (TSelf)this;
		}

		/// <summary>Adds assembly of provided name to entity classes scan list.</summary>
		public TSelf FromAssembly(string assemblyNameString)
		{
			if(String.IsNullOrEmpty(assemblyNameString)) throw new ArgumentNullException("assemblyNameString");
			Contract.EndContractBlock();

			AssembliesToScan.Add(Assembly.Load(assemblyNameString));
			return (TSelf)this;
		}

		/// <summary>Adds base class restriction for found entity types.</summary>
		public TSelf InheritedFrom<T>()
		{
			if (typeof(T).IsInterface)
				throw new ArgumentException(
					String.Format("Type {0} is an interface. Probably you should use Implements<T>() method insted.", typeof(T)));
			Contract.EndContractBlock();

			Predicates.Add(type => typeof(T).IsAssignableFrom(type));
			return (TSelf)this;
		}

		/// <summary>Restricts entities to ones inherited from <typeparamref name="T"/>.</summary>
		public TSelf Implementing<T>()
		{
			if (!typeof(T).IsInterface)
				throw new ArgumentException(
					String.Format("Type {0} is not an interface. Probably you should use InheritedFrom<T>() method insted.", typeof(T)));
			Contract.EndContractBlock();

			Predicates.Add(type => typeof(T).IsAssignableFrom(type));
			return (TSelf) this;
		}

		/// <summary>Adds custom entity type search predicate.</summary>
		public TSelf Where(Predicate<Type> predicate)
		{
			if(predicate == null) throw new ArgumentNullException("predicate");
			Contract.EndContractBlock();

			Predicates.Add(predicate);
			return (TSelf) this;
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