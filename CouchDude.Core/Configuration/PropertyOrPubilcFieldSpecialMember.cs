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
using System.Reflection;

namespace CouchDude.Core.Configuration
{
	/// <summary>Wraps special member implemented as actual property or public field on the entity class.</summary>
	public class PropertyOrPubilcFieldSpecialMember : ISpecialMember
	{
		private readonly Action<object, string> set = (_,__) => { };
		private readonly Func<object, string> get = _ => null;
		private readonly bool isDefined;
		private readonly MemberInfo rawMemberInfo;

		/// <constructor />
		public PropertyOrPubilcFieldSpecialMember(Type entityType, MemberInfo rawMemberInfo) : this(entityType, rawMemberInfo, isSafe: false) { }

		/// <constructor />
		internal protected PropertyOrPubilcFieldSpecialMember(Type entityType, MemberInfo rawMemberInfo, bool isSafe)
		{
			EntityType = entityType;
			this.rawMemberInfo = rawMemberInfo;
			if (rawMemberInfo != null)
			{
				var propertyInfo = rawMemberInfo as PropertyInfo;
				if (propertyInfo != null && (isSafe || IsPropertyOk(propertyInfo)))
				{
					set = (entity, value) => propertyInfo.SetValue(entity, ConvertFromString(value), new object[0]);
					get = entity => ConvertToString(propertyInfo.GetValue(entity, new object[0]));
					isDefined = true;
					return;
				}
				else
				{
					var fieldInfo = rawMemberInfo as FieldInfo;
					if (fieldInfo != null && (isSafe || IsFildOk(fieldInfo)))
					{
						set = (entity, value) => fieldInfo.SetValue(entity, ConvertFromString(value));
						get = entity => ConvertToString(fieldInfo.GetValue(entity));
						isDefined = true;
						return;
					}
					else
						throw new ConfigurationException("Member should be readable and writable property or public field");
				}
			}

			rawMemberInfo = null;
			isDefined = false;
		}

		/// <inheritdoc/>
		public bool IsDefined { get { return isDefined; } }

		/// <inheritdoc/>
		public Type EntityType { get; private set; }

		/// <inheritdoc/>
		public MemberInfo RawMemberInfo { get { return rawMemberInfo; } }

		/// <inheritdoc/>
		public void SetValue(object instance, string value) { set(instance, value); }

		/// <inheritdoc/>
		public string GetValue(object instance) { return get(instance); }
		
		/// <summary>Determines if type is compatible with <see cref="string"/>.</summary>
		protected static bool TypeIsCompatibleWithString(Type type)
		{
			//TODO: add type converter support here
			return type == typeof(string);
		}

		/// <summary>Determines if filed is OK to be used as base of the specal member.</summary>
		protected static bool IsFildOk(FieldInfo fieldInfo)
		{
			return fieldInfo != null && fieldInfo.IsPublic && !fieldInfo.IsStatic;
		}

		/// <summary>Determines if property is OK to be used as base of the specal member.</summary>
		protected static bool IsPropertyOk(PropertyInfo propertyInfo)
		{
			return TypeIsCompatibleWithString(propertyInfo.PropertyType) 
				&& propertyInfo.CanRead 
				&& !propertyInfo.GetGetMethod(nonPublic: true).IsStatic
				&& propertyInfo.CanWrite
				&& !propertyInfo.GetSetMethod(nonPublic: true).IsStatic;
		}

		private static string ConvertToString(object value)
		{
			//TODO: add type converter support here
			return (string)value;
		}

		private static string ConvertFromString(string stringValue)
		{
			//TODO: add type converter support here
			return stringValue;
		}
	}
}