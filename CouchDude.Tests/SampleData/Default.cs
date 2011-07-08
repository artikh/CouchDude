using System;
using CouchDude.Core;
using CouchDude.Core.Configuration;

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
					.CreateSettings()
					.Register(new CustomEntityConfig(typeof(SimpleEntity)))
					.Register(new CustomEntityConfig(typeof(SimpleEntityWithoutRevision))); 
			}
		}

		private class CustomEntityConfig: EntityConfig
		{
			public CustomEntityConfig(Type entityType) : base(entityType) { }

			public override string ConvertEntityIdToDocumentId(string entityId)
			{
				return string.Concat(DocumentType, ".", base.ConvertEntityIdToDocumentId(entityId));
			}

			public override string ConvertDocumentIdToEntityId(string documentId)
			{
				var chunks = documentId.Split('.');
				return base.ConvertDocumentIdToEntityId(chunks[1]);
			}
		}
	}
}
