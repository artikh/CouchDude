using System.Collections.Generic;
using System.Threading.Tasks;

namespace CouchDude
{
	/// <summary>Represents low-level CouchDB API.</summary>
	public interface ICouchApi
	{
		/// <summary>Returns database-specific API object.</summary>
		IDatabaseApi Db(string databaseName);
		
		/// <summary>Requests collection of names of all avaliable databases.</summary>
		Task<ICollection<string>> RequestAllDbNames();
		
		/// <summary>Synchronous version of API.</summary>
		ISynchronousCouchApi Synchronously { get; }
	}
}