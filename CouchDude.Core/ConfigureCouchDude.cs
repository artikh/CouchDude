using System;
using CouchDude.Core.Impl;
using CouchDude.Core.Configuration.Builders;

namespace CouchDude.Core
{
	/// <summary>Start point of fluent configuration of CouchDude.</summary>
	public static class ConfigureCouchDude
	{
		/// <summary>Starts CouchDude configuration.</summary>
		public static SettingsBuilder With()
		{
			return new SettingsBuilder();
		}

		/// <summary>Creates session factory from provided setting.</summary>
		public static ISessionFactory CreateSessionFactory(this Settings settings)
		{
			return new CouchSessionFactory(settings);
		}

		/// <summary>Retrives service from service provider using type argument.</summary>
		public static TService GetService<TService>(this IServiceProvider serviceProvider)
		{
			return (TService) serviceProvider.GetService(typeof (TService));
		}
	}
}