using System;

namespace CouchDude.Core.Conventions
{
	/// <summary>Detects document type for given entities.</summary>
	/// <remarks>Implementers shoud take into considerations possible collisions. 
	/// Preferrably mapping should be 1:1.</remarks>
	public interface ITypeConvention
	{
		/// <summary>Returns document type for given entity type.</summary>
		string GetDocumentType(Type entityType);

		/// <summary>Returns entity type for given document type.</summary>
		Type GetEntityType(string documentType);
	}
}