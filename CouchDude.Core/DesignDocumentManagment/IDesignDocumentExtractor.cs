using System.Collections.Generic;
using System.IO;

namespace CouchDude.Core.DesignDocumentManagment
{
	/// <summary>Design documents extractor interface.</summary>
	public interface IDesignDocumentExtractor 
	{
		/// <summary>Extracts design documents.</summary>
		IDictionary<string, DesignDocument> Extract(TextReader textReader);
	}
}