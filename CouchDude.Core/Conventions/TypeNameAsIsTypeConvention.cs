using System;
using System.Collections.Generic;
using System.Reflection;

namespace CouchDude.Core.Conventions
{
	/// <summary>Base class scannig all types based on their base type.</summary>
	public class TypeNameAsIsTypeConvention : TypeConventionBase
	{
		/// <constructor />
		public TypeNameAsIsTypeConvention(IEnumerable<Assembly> assembliesToScanToScan, ICollection<Type> baseTypes = null)
			: base(assembliesToScanToScan, baseTypes) { }

		/// <summary>Maps entity type to document type.</summary>
		protected internal override string CreateDocumentTypeFromEntityType(Type entityType) { return entityType.Name; }
	}
}