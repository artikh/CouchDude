using System;
using CouchDude.Core.Configuration;

namespace CouchDude.Core.Configuration.Builders
{
	internal struct ScanDescriptor
	{
		public readonly Predicate<Type> Predicate;
		public readonly Func<Type, IEntityConfig> ConfigFactory;

		public ScanDescriptor(Predicate<Type> predicate, Func<Type, IEntityConfig> configFactory)
		{
			Predicate = predicate;
			ConfigFactory = configFactory;
		}
	}
}