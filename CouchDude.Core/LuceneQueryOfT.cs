using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchDude.Core
{
	/// <summary>Typed lucene-couchdb query.</summary>
	public class LuceneQuery<T> : LuceneQuery, IQuery<LuceneResultRow, T>
	{
		/// <summary>Processes raw query result producing meningfull results.</summary>
		public Func<IEnumerable<LuceneResultRow>, IEnumerable<T>> ProcessRows { get; set; }
	}
}
