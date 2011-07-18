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
using System.Collections.Generic;
using System.Reflection;
using CouchDude.Core.Utils;

namespace CouchDude.Core.Configuration
{
	/// <summary>Entity configuration item.</summary>
	public interface IEntityConfig
	{
		/// <summary>Exec type of the entity.</summary>
		Type EntityType { get; }

		/// <summary>Document type as it shows in database.</summary>
		string DocumentType { get; }

		/// <summary>Converts document ID to entity ID.</summary>
		string ConvertDocumentIdToEntityId(string documentId);

		/// <summary>Converts entity ID to document ID.</summary>
		string ConvertEntityIdToDocumentId(string entityId);

		/// <summary>Detects if special ID member have been found on entity type.</summary>
		bool IsIdMemberPresent { get; }

		/// <summary>Sets entity ID property.</summary>
		void SetId(object entity, string entityId);

		/// <summary>Sets entity ID property.</summary>
		string GetId(object entity);

		/// <summary>Detects if special revision member have been found on entity type.</summary>
		bool IsRevisionPresent { get; }

		/// <summary>Sets entity revision property.</summary>
		void SetRevision(object entity, string entityRevision);

		/// <summary>Sets entity revision property.</summary>
		string GetRevision(object entity);

		/// <summary>Retruns entity type members that should be ignored on 
		/// serialization and deserialization.</summary>
		IEnumerable<MemberInfo> IgnoredMembers { get; }
	}
}
