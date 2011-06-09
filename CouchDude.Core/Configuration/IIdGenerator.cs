namespace CouchDude.Core.Configuration
{
	/// <summary>Document ID generator interface.</summary>
	public interface IIdGenerator
	{
		/// <summary>Generates new ID for the document.</summary>
		string GenerateId();
	}
}