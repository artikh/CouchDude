using System;
using System.Runtime.Serialization;
using System.Text;
using CouchDude.Core.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core
{
    /// <summary>Exception thrown in case of missing _rev property on
    /// CouchDB document.</summary>
    [Serializable]
    public class DocumentRevisionMissingException : ParseException
    {
        /// <constructor />
        public DocumentRevisionMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        /// <constructor />
        public DocumentRevisionMissingException(JObject document) : base(GenerateMessage(document)) { }

        private static string GenerateMessage(JToken document = null)
        {
            var message = new StringBuilder("Required field '")
                .Append(EntitySerializer.RevisionPropertyName)
                .Append("' have not found on document. ")
                .Append("Document revision should be supplied by CouchDB.");
            if (document != null)
                message.AppendLine().Append(document.ToString(Formatting.Indented));

            return message.ToString();
        }
    }
}