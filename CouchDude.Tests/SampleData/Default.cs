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
						.TranslatingEntityIdToDocumentIdAs(
							(id, type, documentType) => string.Concat(documentType, ".", id))
						.TranslatingDocumentIdToEntityIdAs((id, type, entityType) => id.Split('.')[1])
					.CreateSettings(); 
			}
		}
	}
}
