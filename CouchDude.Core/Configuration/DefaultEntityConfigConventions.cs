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
			Type entityType, string[] memberNames, ConcurrentDictionary<Type, MemberInfo> cache)
		{
			return cache.GetOrAdd(entityType, et => GetMember(entityType, memberNames)); 
		}

		private static MemberInfo GetMember(IReflect entityType, string[] memberNames)
		{
			var publicProperties =
				from n in memberNames
				select entityType.GetProperty(n, BindingFlags.Instance | BindingFlags.Public) into p
				where p != null && TypeIsCompatibleWithString(p.PropertyType)
				select p;

			var privateProperties =
				from n in memberNames
				select entityType.GetProperty(n, BindingFlags.Instance | BindingFlags.NonPublic) into p
				where p != null && TypeIsCompatibleWithString(p.PropertyType)
				select p;

			var publicFields =
				from n in memberNames
				select entityType.GetField(n, BindingFlags.Instance | BindingFlags.Public) into f
				where f != null && TypeIsCompatibleWithString(f.FieldType)
				select f;

			return publicProperties.OfType<MemberInfo>().Concat(privateProperties).Concat(publicFields).FirstOrDefault();
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

		public static bool IsEntityRevisionMemberPresent(Type entityType)
		{
			return GetRevisionMember(entityType) != null;
		}

		public static string GetEntityRevision(object entity, Type entityType)
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

		public static void SetEntityRevision(string revision, object entity, Type entityType)
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

		public static bool IsEntityIdMemberPresent(Type entityType)
		{
			return GetIdMember(entityType) != null;
		}

		public static string GetEntityId(object entity, Type entityType)
		{
			Contract.Requires(entityType != null);
			Contract.Requires(entity != null);
			Contract.Requires(entityType.IsAssignableFrom(entity.GetType()));

			var idMember = GetIdMember(entityType);
			if (idMember != null)
			{
				var idValue = GetValue(idMember, entity);
				return ConvertToString(idValue);
			}
			
			return null;
		}

		public static void SetEntityId(string id, object entity, Type entityType)
		{
			Contract.Requires(entityType != null);
			Contract.Requires(entity != null);
			Contract.Requires(entityType.IsAssignableFrom(entity.GetType()));

			var idMember = GetIdMember(entityType);
			if (idMember != null)
			{
				var idValue = ConvertFromString(id);
				SetValue(idMember, entity, idValue);
			}
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

		public static IEnumerable<MemberInfo> GetIgnoredMembers(Type entityType)
		{
			var idMember = GetIdMember(entityType);
			if (idMember != null)
				yield return idMember;
			var revisionMember = GetRevisionMember(entityType);
			if (revisionMember != null)
				yield return revisionMember;
		}
	}
}