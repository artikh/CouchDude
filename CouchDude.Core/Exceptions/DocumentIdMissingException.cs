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
using CouchDude.Core.Api;

namespace CouchDude.Core
{
	/// <summary>Exception thrown in case of missing _id property on
	/// CouchDB document.</summary>
	[Serializable]
	public class DocumentIdMissingException : ParseException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="DocumentIdMissingException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public DocumentIdMissingException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public DocumentIdMissingException(IDocument document) : base(GenerateMessage(document)) { }

		private static string GenerateMessage(IDocument document = null)
		{
			var message = new StringBuilder("Required field '")
				.Append(Document.IdPropertyName)
				.Append("' have not found on document. ")
				.Append("Documents id is required by CouchDB.");
			if (document != null)
				message.AppendLine().Append(document.ToString());

			return message.ToString();
		}
	}
}
