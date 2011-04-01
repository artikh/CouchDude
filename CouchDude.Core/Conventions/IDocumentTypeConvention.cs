using System;

namespace CouchDude.Core.Conventions
{
	/// <summary>Detects document type for given entities.</summary>
	public interface IDocumentTypeConvention
	{
		/// <summary>Returns entity type.</summary>
		string GetType(Type entityType);
	}
}