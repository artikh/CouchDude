using System.Collections.Generic;

namespace CouchDude
{
	/// <summary>Wraps lists of names and roles.</summary>
	public class NamesRoles
	{
		/// <summary>User names</summary>
		public ICollection<string> Names { get; set; }

		/// <summary>User roles</summary>
		public ICollection<string> Roles { get; set; }
	}
}