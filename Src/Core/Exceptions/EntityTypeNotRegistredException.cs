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

namespace CouchDude
{
	/// <summary>Thrown in case of unregistred entity type.</summary>
	[Serializable]
	public class EntityTypeNotRegistredException : ConfigurationException
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
		public EntityTypeNotRegistredException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public EntityTypeNotRegistredException(Type entityType) : base(GenerateMessage(entityType)) { }

		private static string GenerateMessage(Type entityType)
		{
			return string.Format("Type {0} have not been registred.", entityType);
		}
	}

	/// <summary>Thrown in case of unregistred entity type.</summary>
	[Serializable]
	public class DocumentNotFoundException : ConfigurationException
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
		public DocumentNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public DocumentNotFoundException(string documentId, string revision) : base(GenerateMessage(documentId, revision)) { }

		private static string GenerateMessage(string documentId, string revision)
		{
			return string.Format("Document {0}{1} have not found", documentId, string.Format("(rev:{0})", revision));
		}
	}
}