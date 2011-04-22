using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace CouchDude.Core.Conventions
{
	/// <summary>Custom type name convention - provides choise to the user.</summary>
	public class CustomTypeConvention : TypeConventionBase
	{
		private readonly Func<Type, string> createDocumentTypeFromEntityType;

		/// <constructor />
		public CustomTypeConvention(
			IEnumerable<Assembly> assembliesToScan, ICollection<Type> baseTypes, Func<Type, string> createDocumentTypeFromEntityType)
			: base(assembliesToScan, baseTypes)
		{
			if (createDocumentTypeFromEntityType == null) throw new ArgumentNullException("createDocumentTypeFromEntityType");
			Contract.EndContractBlock();

			this.createDocumentTypeFromEntityType = createDocumentTypeFromEntityType;
		}

		/// <inheritdoc/>
		protected internal override string CreateDocumentTypeFromEntityType(Type entityType)
		{
			return createDocumentTypeFromEntityType(entityType);
		}
	}
}