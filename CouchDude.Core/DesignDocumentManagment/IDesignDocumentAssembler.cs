using System.Collections.Generic;

namespace CouchDude.Core.DesignDocumentManagment
{
	/// <summary>Design document assembler interface.</summary>
	public interface IDesignDocumentAssembler 
	{
		/// <summary>Geterates design documents form file system structures.</summary>
		IDictionary<string, DesignDocument> Assemble();
	}
}