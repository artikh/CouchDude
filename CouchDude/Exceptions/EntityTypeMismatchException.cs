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
	/// <summary> EntityTypeMismatch exception.</summary>
	[Serializable]
	public class EntityTypeMismatchException : CouchDudeException
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
		protected EntityTypeMismatchException(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <constructor />
		public EntityTypeMismatchException(string documentType, Type entityType)
			: base(GenerateMessage(documentType, entityType)) { }

		/// <constructor />
		public EntityTypeMismatchException(Type cachedEntityType, Type entityType)
			: base(GenerateMessage(cachedEntityType, entityType)) { }

		private static string GenerateMessage(string documentType, Type entityType)
		{
			return string.Format(
				"Document type '{0}' is incompatible with entity type {1}", 
				documentType, 
				entityType.AssemblyQualifiedName);
		}

		private static string GenerateMessage(Type cachedEntityType, Type entityType)
		{
			return string.Format(
				"Cached entity type '{0}' is incompatible with entity type {1}",
				cachedEntityType.AssemblyQualifiedName, 
				entityType.AssemblyQualifiedName);
		}
	}
}
