using System;

namespace CouchDude.Core.Conventions
{
	/// <summary>Implementors should search for special properties on 
	/// entity types for framework to set and get.</summary>
	public interface ISpecialPropertyConvention
	{
		/// <summary>Returns special property descriptor.</summary>
		SpecialPropertyDescriptor Get(Type entityType);
	}
}