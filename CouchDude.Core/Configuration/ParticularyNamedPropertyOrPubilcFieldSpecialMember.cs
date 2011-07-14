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