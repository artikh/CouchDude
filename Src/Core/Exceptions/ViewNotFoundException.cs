using System;
using System.Runtime.Serialization;

namespace CouchDude
{
	/// <summary>Exception thrown if no view specified in query was found in DB.</summary>
	public class ViewNotFoundException : QueryException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="ViewNotFoundException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public ViewNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) {}

		/// <constructor />
		public ViewNotFoundException(ViewQuery query)
			: base("View {0}/{1} is not declared in database. Perhaps design document _design/{0} should be updated",
				query.DesignDocumentName,
				query.ViewName) { }
	}
}