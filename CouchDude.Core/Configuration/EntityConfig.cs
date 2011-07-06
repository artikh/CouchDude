using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace CouchDude.Core.Configuration
{
	/// <summary>Delegate type for <see cref="EntityConfig.EntityTypeToDocumentType"/> convention.</summary>
	public delegate string EntityTypeToDocumentTypeConvention(Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.EntityIdToDocumentId"/> convention.</summary>
	public delegate string EntityIdToDocumentIdConvention(string entityId, Type entityType, string documentType);

	/// <summary>Delegate type for <see cref="EntityConfig.DocumentIdToEntityId"/> convention.</summary>
	public delegate string DocumentIdToEntityIdConvention(string documentId, string documentType, Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.TrySetEntityId"/> convention.</summary>
	public delegate bool TrySetEntityIdConvention(string id, object entity, Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.TryGetEntityId"/> convention.</summary>
	public delegate bool TryGetEntityIdConvention(object entity, Type entityType, out string entityId);

	/// <summary>Delegate type for <see cref="EntityConfig.SetEntityRevision"/> convention.</summary>
	public delegate void SetEntityRevisionConvention(string revision, object entity, Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.GetEntityRevision"/> convention.</summary>
	public delegate string GetEntityRevisionConvention(object entity, Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.GetIgnoredMembers"/> convention.</summary>
	public delegate IEnumerable<MemberInfo> GetIgnoredMembersConvention(Type entityType);

	/// <summary>Default entity configuration object delegating all actions to public field delegates.</summary>
	public class EntityConfig : IEntityConfig
	{
		/// <summary>Entity type to document type conversion algorithm.</summary>
		public static EntityTypeToDocumentTypeConvention EntityTypeToDocumentType = DefaultEntityConfigConventions.EntityTypeToDocumentType;
		
		/// <summary>Converts entity ID to document ID. Supplied with entity type and document type.</summary>
		public static EntityIdToDocumentIdConvention EntityIdToDocumentId = DefaultEntityConfigConventions.EntityIdToDocumentId;

		/// <summary>Converts document ID to entity ID. Supplied with and document type entity type.</summary>
		public static DocumentIdToEntityIdConvention DocumentIdToEntityId = DefaultEntityConfigConventions.DocumentIdToEntityId;

		/// <summary>Set's ID property on entity.</summary>
		public static TrySetEntityIdConvention TrySetEntityId = DefaultEntityConfigConventions.TrySetEntityId;

		/// <summary>Get's ID property of entity.</summary>
		public static TryGetEntityIdConvention TryGetEntityId = DefaultEntityConfigConventions.TryGetEntityId;

		/// <summary>Set's revision property on entity.</summary>
		public static SetEntityRevisionConvention SetEntityRevision = DefaultEntityConfigConventions.SetEntityRevisionIfPosssible;

		/// <summary>Get's revision property of entity.</summary>
		public static GetEntityRevisionConvention GetEntityRevision = DefaultEntityConfigConventions.GetEntityRevisionIfPossible;

		/// <summary>Get's revision property of entity.</summary>
		public static GetIgnoredMembersConvention GetIgnoredMembers = DefaultEntityConfigConventions.GetIgnoredMembers;
		
		/// <constructor />
		public EntityConfig(Type entityType)
		{
			if (entityType == null) throw new ArgumentNullException("entityType");
			Contract.EndContractBlock();

			EntityType = entityType;
			DocumentType = EntityTypeToDocumentType(entityType);
			IgnoredMembers = GetIgnoredMembers(entityType) ?? new Type[0];
		}

		/// <inheritdoc/>
		public Type EntityType { get; private set; }

		/// <inheritdoc/>
		public string DocumentType { get; private set; }

		/// <inheritdoc/>
		public string ConvertDocumentIdToEntityId(string documentId)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			Contract.EndContractBlock();

			return DocumentIdToEntityId(documentId, DocumentType, EntityType);
		}

		/// <inheritdoc/>
		public string ConvertEntityIdToDocumentId(string entityId)
		{
			if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("entityId");
			Contract.EndContractBlock();

			return EntityIdToDocumentId(entityId, EntityType, DocumentType);
		}

		/// <inheritdoc/>
		public bool TrySetId(object entity, string entityId)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("entityId");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			return TrySetEntityId(entityId, entity, EntityType);
		}

		/// <inheritdoc/>
		public bool TryGetId(object entity, out string entityId)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			return TryGetEntityId(entity, EntityType, out entityId);
		}

		/// <inheritdoc/>
		public void SetRevision(object entity, string entityRevision)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (string.IsNullOrEmpty(entityRevision)) throw new ArgumentNullException("entityRevision");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			SetEntityRevision(entityRevision, entity, EntityType);
		}

		/// <inheritdoc/>
		public string GetRevision(object entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			return GetEntityRevision(entity, EntityType);
		}

		/// <inheritdoc/>
		public IEnumerable<MemberInfo> IgnoredMembers { get; private set; }
	}
}