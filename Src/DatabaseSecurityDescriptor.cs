using System.Collections.Generic;

namespace CouchDude
{
	/// <summary>Database securtity object.</summary>
	public class DatabaseSecurityDescriptor
	{
		/// <summary>Database admin desciptors.</summary>
		public NamesRoles Admins { get; set; }

		/// <summary>Database reader desciptors.</summary>
		public NamesRoles Readers { get; set; }
	}
}