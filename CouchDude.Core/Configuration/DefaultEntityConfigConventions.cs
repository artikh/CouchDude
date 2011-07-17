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
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using CouchDude.Core.Utils;

namespace CouchDude.Core.Configuration
{
	internal static class DefaultEntityConfigConventions
	{
		private static readonly string[] RevisionMemberNames = new[] {"Rev", "Revision"};

		private static readonly string[] IdMembersNames = new[] {"Id", "ID"};

		private static readonly ConcurrentDictionary<Type, ISpecialMember> IdMembers =
			new ConcurrentDictionary<Type, ISpecialMember>();

		private static readonly ConcurrentDictionary<Type, ISpecialMember> RevisionMembers =
			new ConcurrentDictionary<Type, ISpecialMember>();

		private static ISpecialMember GetMemberOfName(
			Type entityType, string[] memberNames, ConcurrentDictionary<Type, ISpecialMember> cache)
		{
			return cache.GetOrAdd(entityType, et => new ParticularyNamedPropertyOrPubilcFieldSpecialMember(entityType, memberNames)); 
		}


		public static ISpecialMember GetRevisionMember(Type entityType)
		{
			return GetMemberOfName(entityType, RevisionMemberNames, RevisionMembers);
		}

		public static ISpecialMember GetIdMember(Type entityType)
		{
			return GetMemberOfName(entityType, IdMembersNames, IdMembers);
		}

		public static string EntityIdToDocumentId(string entityId, Type entityType, string documentType)
		{
			return entityId;
		}

		public static string DocumentIdToEntityId(string documentId, string documentType, Type entityType)
		{
			return documentId;
		}

		public static string EntityTypeToDocumentType(Type entityType)
		{
			return entityType.Name.ToCamelCase();
		}
	}
}