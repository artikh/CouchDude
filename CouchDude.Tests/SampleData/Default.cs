using CouchDude.Core;

namespace CouchDude.Tests.SampleData
{
	static class Default
	{
		public static Settings Settings = ConfigureCouchDude.With()
			.ServerUri("http://localhost:5984")
			.DatabaseName("test")
			.MappingEntities()
				.FromAssemblyOf<IEntity>()
				.Imprementing<IEntity>()
				.ToDocumentTypeCamelCase()
			.CreateSettings();
	}
}
