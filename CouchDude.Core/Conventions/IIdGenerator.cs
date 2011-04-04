using System;
using CouchDude.Core.Implementation;

namespace CouchDude.Core.Conventions
{
	/// <summary>Document ID generator interface.</summary>
	public interface IIdGenerator
	{
		/// <summary>Generates new ID for the document.</summary>
		string GenerateId();
	}
}