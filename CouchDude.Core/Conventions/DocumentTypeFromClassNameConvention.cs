using System;

namespace CouchDude.Core.Conventions
{
	/// <summary>Provides entity type names to document type convention.</summary>
	public class DocumentTypeFromClassNameConvention: IDocumentTypeConvention 
	{
		/// <inheritdoc/>
		public string GetType(Type entityType)
		{
			var typeName = entityType.Name;
			if (Char.IsUpper(typeName[0]))
				typeName = Char.ToLower(typeName[0]) + typeName.Substring(1);

			return typeName;
		}
	}
}