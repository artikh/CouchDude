using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchDude.Core
{
	/// <summary>Typed lucene-couchdb query.</summary>
	public class LuceneQuery<T> : LuceneQuery
	{
		/// <summary>Processes raw query result producing meningfull results.</summary>
		public readonly Func<IEntityConfigRepository, IEnumerable<LuceneResultRow>, IEnumerable<T>> ProcessRows = ProcessResultDefault;
		
		private static IEnumerable<T> ProcessResultDefault(IEntityConfigRepository entityConfigRepository, IEnumerable<LuceneResultRow> rawViewResults)
		{
			var entityConfig = entityConfigRepository.TryGetConfig(typeof(T));
			if(entityConfig != null)
			{
				return 
					from row in rawViewResults
					select row.Document into document
					select document == null ? default(T) : (T)document.TryDeserialize(entityConfig);
			}
			else
				return
					from row in rawViewResults
					select row.Fields into value
					select value == null ? default(T) : (T)value.TryDeserialize(typeof(T));
		}
	}
}
