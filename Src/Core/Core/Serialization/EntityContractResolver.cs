#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																				
																																					
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
using CouchDude.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CouchDude.Serialization
{
	/// <summary>Detects circural reference detection and ignored member support.</summary>
	public class EntityContractResolver : JsonFragment.CamelCasePrivateSetterPropertyContractResolver
	{
		private readonly Type entityType;
		private readonly ISet<MemberInfo> ignoredMembers;

		/// <constructor />
		public EntityContractResolver(Type entityType, IEnumerable<MemberInfo> ignoredMembers)
		{
			this.entityType = entityType;
			this.ignoredMembers = new HashSet<MemberInfo>(ignoredMembers);
		}

		/// <inheritdoc />
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var jsonProperty = base.CreateProperty(member, memberSerialization);

			if (jsonProperty.PropertyType == entityType)
				throw new InvalidOperationException(
					String.Format(
						"Entity {0} references itself (maybe indirectly). This configuration is unsupported by CouchDude yet.",
						entityType.AssemblyQualifiedName));

			if (ignoredMembers.Contains(member))
				jsonProperty.Ignored = true;

			return jsonProperty;
		}
	}
}