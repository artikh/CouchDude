using System;
using System.Reflection;

namespace CouchDude.Core.Conventions
{
	/// <summary>Maps entities to documets camelCasing their type names.</summary>
	public class CamelCaseTypeNameToConvention: TypeNameTypeConvention
	{
		/// <constructor />
		public CamelCaseTypeNameToConvention(Assembly[] assembliesToScan, Type[] baseTypes = null) 
			: base(assembliesToScan, baseTypes) { }

		/// <summary>Maps entity type to document type.</summary>
		protected override string CreateDocumentTypeFromEntityType(Type entityType)
		{
			var documentType = entityType.Name;
			if (Char.IsUpper(documentType[0]))
				documentType = Char.ToLower(documentType[0]) + documentType.Substring(1);
			return documentType;
		}
	}
}