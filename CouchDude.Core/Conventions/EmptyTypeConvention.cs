using System;

namespace CouchDude.Core.Conventions
{
	internal class EmptyTypeConvention : ITypeConvention
	{
		/// <inheritdoc/>
		public string GetDocumentType(Type entityType)
		{
			return null;
		}

		/// <inheritdoc/>
		public Type GetEntityType(string documentType)
		{
			return null;
		}
	}
}