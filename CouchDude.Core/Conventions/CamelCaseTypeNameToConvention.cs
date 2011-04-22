using System;
using System.Collections.Generic;
using System.Reflection;

namespace CouchDude.Core.Conventions
{
	/// <summary>Maps entities to documets camelCasing their type names.</summary>
	public class CamelCaseTypeNameToConvention: TypeConventionBase
	{
		/// <constructor />
		public CamelCaseTypeNameToConvention(IEnumerable<Assembly> assembliesToScanToScan, ICollection<Type> baseTypes = null) 
			: base(assembliesToScanToScan, baseTypes) { }

		/// <summary>Maps entity type to document type.</summary>
		protected internal override string CreateDocumentTypeFromEntityType(Type entityType)
		{
			var documentType = entityType.Name;
			if (Char.IsUpper(documentType[0]))
				documentType = Char.ToLower(documentType[0]) + documentType.Substring(1);
			return documentType;
		}
	}
}