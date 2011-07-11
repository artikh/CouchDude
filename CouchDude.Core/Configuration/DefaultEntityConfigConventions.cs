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