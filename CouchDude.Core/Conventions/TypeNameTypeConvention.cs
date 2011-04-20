using System;
using System.Linq;
using System.Reflection;

namespace CouchDude.Core.Conventions
{
	/// <summary>Base class scannig all types based on their base type.</summary>
	public class TypeNameTypeConvention : TypeConventionBase
	{
		/// <summary>Base type and/or interfaces to be requried for entity type.</summary>
		private readonly Type[] baseTypes;

		/// <constructor />
		public TypeNameTypeConvention(Assembly[] assembliesToScan, Type[] baseTypes = null) : base(assembliesToScan)
		{
			this.baseTypes = baseTypes;
		}

		/// <constructor />
		protected internal override string ProcessType(Type entityType)
		{
			if (baseTypes != null && baseTypes.Length > 0)
				foreach (var expectedBaseType in baseTypes)
					if (!expectedBaseType.IsAssignableFrom(entityType)) 
						return null;

			var documentType = CreateDocumentTypeFromEntityType(entityType);

			var previouslyRegistredEntityType = base.GetEntityType(documentType);
			if (previouslyRegistredEntityType != null)
				throw new ConventionException(
					"Document type '{0}' could not be registred for entity {1}: it has been registred for entity {2} already.",
					documentType,
					entityType,
					previouslyRegistredEntityType);

			return documentType;
		}

		/// <summary>Maps entity type to document type.</summary>
		protected virtual string CreateDocumentTypeFromEntityType(Type entityType) { return entityType.Name; }
	}
}