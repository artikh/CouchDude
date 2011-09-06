using System.Collections.Generic;

namespace CouchDude
{
	/// <summary>Synchronous version of low-level CouchDB API.</summary>
	public interface ISynchronousCouchApi
	{
		/// <summary>Returns collection of names of all avaliable databases.</summary>
		ICollection<string> RequestAllDbNames();
	}
}