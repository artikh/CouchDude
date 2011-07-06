using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using CouchDude.Core.Utils;

namespace CouchDude.Core.Configuration
{
	internal static class DefaultEntityConfigConventions
	{
		private static readonly string[] RevisionMemberNames = new[] {"Rev", "Revision"};

		private static readonly string[] IdMembersNames = new[] {"Id", "ID"};

		private static readonly ConcurrentDictionary<Type, MemberInfo> IdMembers =
			new ConcurrentDictionary<Type, MemberInfo>();

		private static readonly ConcurrentDictionary<Type, MemberInfo> RevisionMembers =
			new ConcurrentDictionary<Type, MemberInfo>(); 

		private static MemberInfo GetRevisionMember(Type entityType)
		{
			return GetMemberOfName(entityType, RevisionMemberNames, RevisionMembers);
		}

		private static MemberInfo GetIdMember(Type entityType)
		{
			return GetMemberOfName(entityType, IdMembersNames, IdMembers);
		}

		private static MemberInfo GetMemberOfName(
			Type entityType, IEnumerable<string> memberNames, ConcurrentDictionary<Type, MemberInfo> cache)
		{
			return cache.GetOrAdd(
				entityType,
				et => memberNames
				  .Select(memberName => GetMember(entityType, memberName))
				  .FirstOrDefault(revisionMember => revisionMember != null));
		}

		private static MemberInfo GetMember(Type entityType, string memberName)
		{
			var propertyInfo = entityType.GetProperty(
				memberName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (propertyInfo != null && TypeIsCompatibleWithString(propertyInfo.PropertyType))
				return propertyInfo;
			var fieldInfo = entityType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo != null && TypeIsCompatibleWithString(fieldInfo.FieldType))
				return fieldInfo;
			return null;
		}

		private static object GetValue(MemberInfo memberInfo, object entity)
		{
			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
				return propertyInfo.GetValue(entity, new object[0]);

			var fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
				return fieldInfo.GetValue(entity);

			return null;
		}

		private static void SetValue(MemberInfo memberInfo, object entity, object value)
		{
			var propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				propertyInfo.SetValue(entity, value, new object[0]);
				return;
			}

			var fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
			{
				fieldInfo.SetValue(entity, value);
				return;
			}
		}

		private static bool TypeIsCompatibleWithString(Type type)
		{
			//TODO: add type converter support here
			return type == typeof (string);
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

		public static string GetEntityRevisionIfPossible(object entity, Type entityType)
		{
			Contract.Requires(entityType != null);
			Contract.Requires(entity != null);
			Contract.Requires(entityType.IsAssignableFrom(entity.GetType()));

			var revisionMember = GetRevisionMember(entityType);
			if (revisionMember != null)
			{
				var revisionValue = GetValue(revisionMember, entity);
				var revision = ConvertToString(revisionValue);
				return revision;
			}
			return null;
		}

		public static void SetEntityRevisionIfPosssible(string revision, object entity, Type entityType)
		{
			Contract.Requires(entityType != null);
			Contract.Requires(entity != null);
			Contract.Requires(entityType.IsAssignableFrom(entity.GetType()));

			var revisionMember = GetRevisionMember(entityType);
			if (revisionMember != null)
			{
				var revisionValue = ConvertFromString(revision);
				SetValue(revisionMember, entity, revisionValue);
			}
		}

		public static bool TryGetEntityId(object entity, Type entityType, out string entityId)
		{
			Contract.Requires(entityType != null);
			Contract.Requires(entity != null);
			Contract.Requires(entityType.IsAssignableFrom(entity.GetType()));

			var idMember = GetIdMember(entityType);
			if (idMember == null)
			{
				entityId = null;
				return false;
			}
			else
			{
				var idValue = GetValue(idMember, entity);
				entityId = ConvertToString(idValue);
				return true;
			}
		}

		public static bool TrySetEntityId(string id, object entity, Type entityType)
		{
			Contract.Requires(entityType != null);
			Contract.Requires(entity != null);
			Contract.Requires(entityType.IsAssignableFrom(entity.GetType()));

			var idMember = GetIdMember(entityType);
			if (idMember != null)
			{
				var idValue = ConvertFromString(id);
				SetValue(idMember, entity, idValue);
				return true;
			}
			return false;
		}

		public static string DocumentIdToEntityId(string documentId, string documentType, Type entityType)
		{
			return documentId;
		}
		
		public static string EntityIdToDocumentId(string entityId, Type entityType, string documentType)
		{
			return entityId;
		}

		public static string EntityTypeToDocumentType(Type entityType)
		{
			return entityType.Name.ToCamelCase();
		}

		public static IEnumerable<MemberInfo> GetIgnoredMembers(Type entityType)
		{
			return new MemberInfo[0];
		}
	}
}