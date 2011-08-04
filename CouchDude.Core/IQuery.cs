using System;
using System.Collections.Generic;

namespace CouchDude.Core
{
	/// <summary>CouchDB query common members.</summary>
	public interface IQuery<TRow, TResult>
	{
		/// <summary>Design document name (id without '_design/' prefix) to use view from.</summary>
		string DesignDocumentName { get; set; }

		/// <summary>Indicates that we need documents from couchdb in result.</summary>
		bool IncludeDocs { get; set; }

		/// <summary>Maximum rows should be returned from database.</summary>
		int? Limit { get; set; }

		/// <summary>Rows should be skipped before first being returned.</summary>
		int? Skip { get; set; }

		/// <summary>Query result row to result transform operation. If <c>null</c> default one gets used.</summary>
		Func<IEnumerable<TRow>, IEnumerable<TResult>> ProcessRows { get; set; }

		/// <summary>Expreses query as relative URL.</summary>
		Uri ToUri();
	}
}