#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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

namespace CouchDude
{
	/// <summary>Thrown in case of unregistred document type.</summary>
	[Serializable]
	public class DocumentTypeNotRegistredException : ConfigurationException
	{
		/// <summary>Initializes a new instance of the 
		/// <see cref="EntityTypeMismatchException" /> class.</summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the 
		/// serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains 
		/// contextual information about the source or destination.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="info"/> 
		/// parameter is null. </exception>
		/// <exception cref="SerializationException">The class name is null or 
		/// <see cref="Exception.HResult"/> is zero (0). </exception>
		public DocumentTypeNotRegistredException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public DocumentTypeNotRegistredException(string documentType) : base(GenerateMessage(documentType)) { }

		private static string GenerateMessage(string documentType)
		{
			return string.Format("Unknown document type '{0}'.", documentType);
		}
	}
}