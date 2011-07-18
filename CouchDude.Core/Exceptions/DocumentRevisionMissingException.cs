#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

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