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
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace CouchDude.Core.Configuration
{
	/// <summary>Finds and manipulates with special member declared as </summary>
	public class ParticularyNamedPropertyOrPubilcFieldSpecialMember : PropertyOrPubilcFieldSpecialMember
	{
		/// <constructor />
		public ParticularyNamedPropertyOrPubilcFieldSpecialMember(
			Type entityType, params string[] possibleNames) : base(entityType, GetMember(entityType, possibleNames), isSafe: true) { }

		private static MemberInfo GetMember(IReflect entityType, string[] memberNames)
		{
			var publicProperties =
				from name in memberNames
				select entityType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public) into propertyInfo
				where propertyInfo != null && IsPropertyOk(propertyInfo)
				select propertyInfo;

			var privateProperties =
				from name in memberNames
				select entityType.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic) into propertyInfo
				where propertyInfo != null && IsPropertyOk(propertyInfo)
				select propertyInfo;

			var publicFields =
				from name in memberNames
				select entityType.GetField(name, BindingFlags.Instance | BindingFlags.Public) into fieldInfo
				where fieldInfo != null && IsFildOk(fieldInfo)
				select fieldInfo;

			return publicProperties.OfType<MemberInfo>().Concat(privateProperties).Concat(publicFields).FirstOrDefault();
		}
	}
}