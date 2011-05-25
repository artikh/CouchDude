using CouchDude.Core;

namespace CouchDude.Tests.SampleData
{
	static class Default
	{
		public static Settings Settings
		{
			get 
			{ 
				return ConfigureCouchDude.With()
					.ServerUri("http://127.0.0.1:5984")
					.DatabaseName("test")
					.MappingEntities()
					.FromAssemblyOf<IEntity>()
					.Implementing<IEntity>()
					.ToDocumentTypeCamelCase()
					.CreateSettings(); 
			}
		}
	}
}
