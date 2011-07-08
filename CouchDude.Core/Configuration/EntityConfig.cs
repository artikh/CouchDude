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

	/// <summary>Delegate type for <see cref="EntityConfig.IsEntityIdMemberPresent"/> convention.</summary>
	public delegate bool IsEntityIdMemberPresentConvention(Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.SetEntityId"/> convention.</summary>
	public delegate void SetEntityIdConvention(string id, object entity, Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.GetEntityId"/> convention.</summary>
	public delegate string GetEntityIdConvention(object entity, Type entityType);

	/// <summary>Delegate type for <see cref="EntityConfig.IsEntityRevisionMemberPresent"/> convention.</summary>
	public delegate bool IsEntityRevisionMemberPresentConvention(Type entityType);

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

		/// <summary>Detects if ID is present property on entity.</summary>
		public static IsEntityIdMemberPresentConvention IsEntityIdMemberPresent = DefaultEntityConfigConventions.IsEntityIdMemberPresent;

		/// <summary>Set's ID property on entity.</summary>
		public static SetEntityIdConvention SetEntityId = DefaultEntityConfigConventions.SetEntityId;

		/// <summary>Get's ID property of entity.</summary>
		public static GetEntityIdConvention GetEntityId = DefaultEntityConfigConventions.GetEntityId;

		/// <summary>Detects if ID is present property on entity.</summary>
		public static IsEntityRevisionMemberPresentConvention IsEntityRevisionMemberPresent = 
			DefaultEntityConfigConventions.IsEntityRevisionMemberPresent;

		/// <summary>Set's revision property on entity.</summary>
		public static SetEntityRevisionConvention SetEntityRevision = DefaultEntityConfigConventions.SetEntityRevision;

		/// <summary>Get's revision property of entity.</summary>
		public static GetEntityRevisionConvention GetEntityRevision = DefaultEntityConfigConventions.GetEntityRevision;

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
		public virtual Type EntityType { get; private set; }

		/// <inheritdoc/>
		public virtual string DocumentType { get; private set; }

		/// <inheritdoc/>
		public virtual string ConvertDocumentIdToEntityId(string documentId)
		{
			if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException("documentId");
			Contract.EndContractBlock();

			return DocumentIdToEntityId(documentId, DocumentType, EntityType);
		}

		/// <inheritdoc/>
		public virtual string ConvertEntityIdToDocumentId(string entityId)
		{
			if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("entityId");
			Contract.EndContractBlock();

			return EntityIdToDocumentId(entityId, EntityType, DocumentType);
		}

		/// <inheritdoc/>
		public bool IsIdMemberPresent { get { return IsEntityIdMemberPresent(EntityType); } }

		/// <inheritdoc/>
		public virtual void SetId(object entity, string entityId)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("entityId");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			SetEntityId(entityId, entity, EntityType);
		}

		/// <inheritdoc/>
		public virtual string GetId(object entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			return GetEntityId(entity, EntityType);
		}
		
		/// <inheritdoc/>
		public bool IsRevisionPresent { get { return IsEntityRevisionMemberPresent(EntityType); } }

		/// <inheritdoc/>
		public virtual void SetRevision(object entity, string entityRevision)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (string.IsNullOrEmpty(entityRevision)) throw new ArgumentNullException("entityRevision");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			SetEntityRevision(entityRevision, entity, EntityType);
		}

		/// <inheritdoc/>
		public virtual string GetRevision(object entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			return GetEntityRevision(entity, EntityType);
		}

		/// <inheritdoc/>
		public virtual IEnumerable<MemberInfo> IgnoredMembers { get; private set; }
	}
}