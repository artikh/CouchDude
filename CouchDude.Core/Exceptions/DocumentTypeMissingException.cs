using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CouchDude.Core.Impl;

namespace CouchDude.Core
{
	/// <summary>Exception thrown in case of missing type property on
	/// CouchDB document.</summary>
	[Serializable]
    public class DocumentTypeMissingException : ParseException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="DocumentTypeMissingException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public DocumentTypeMissingException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public DocumentTypeMissingException(JObject document) : base(GenerateMessage(document)) { }

		private static string GenerateMessage(JToken document = null)
		{
			var message = new StringBuilder("Required field '")
				.Append(EntitySerializer.TypePropertyName)
				.Append("' have not found on document. ")
				.Append("Type on documents has nothing to do with CouchDB itself, ")
				.Append("however it's required by CouchDude so it colud do it magic stuff.");
			if (document != null)
				message.AppendLine().Append(document.ToString(Formatting.Indented));

			return message.ToString();
		}
	}
}
