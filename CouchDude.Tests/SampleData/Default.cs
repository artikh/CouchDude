using CouchDude.Core;
using CouchDude.Core.Initialization;

namespace CouchDude.Tests.SampleData
{
	static class Default
	{
		public static Settings Settings = Configure.With()
			.ServerUri("http://localhost:5984")
			.DatabaseName("test")
			.MappingEntities()
				.FromAssemblyOf<IEntity>()
				.Imprementing<IEntity>()
				.ToDocumentTypeCamelCase()
			.CreateSettings();
	}
}
