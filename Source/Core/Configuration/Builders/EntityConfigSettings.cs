using System;
using System.Reflection;

namespace CouchDude.Configuration.Builders
{
	class EntityConfigSettings
	{
		public EntityTypeToDocumentTypeConvention EntityTypeToDocumentType;
		public Func<Type, MemberInfo> IDMemberInfoLookup;
		public Func<Type, MemberInfo> RevisionMemberInfoLookup;
		public DocumentIdToEntityIdConvention DocumentIdToEntityId;
		public EntityIdToDocumentIdConvention EntityIdToDocumentId;
		public Func<Type, IEntityConfig> CustomEntityConfigFactory;

		public void Merge(EntityConfigSettings lowerPrioritySettings)
		{
			if (EntityTypeToDocumentType == null)
				EntityTypeToDocumentType = lowerPrioritySettings.EntityTypeToDocumentType;
			if (IDMemberInfoLookup == null)
				IDMemberInfoLookup = lowerPrioritySettings.IDMemberInfoLookup;
			if (RevisionMemberInfoLookup == null)
				RevisionMemberInfoLookup = lowerPrioritySettings.RevisionMemberInfoLookup;
			if (DocumentIdToEntityId == null)
				DocumentIdToEntityId = lowerPrioritySettings.DocumentIdToEntityId;
			if (EntityIdToDocumentId == null)
				EntityIdToDocumentId = lowerPrioritySettings.EntityIdToDocumentId;
			if (CustomEntityConfigFactory == null)
				CustomEntityConfigFactory = lowerPrioritySettings.CustomEntityConfigFactory;
		}

		public IEntityConfig Create(Type entityType)
		{
			if(CustomEntityConfigFactory != null)
			{
				var customEntityConfig = CustomEntityConfigFactory(entityType);
				if (customEntityConfig != null)
					return customEntityConfig;
			}

			PropertyOrPubilcFieldSpecialMember idMember;
			if (IDMemberInfoLookup != null)
			{
				var idMemberInfo = IDMemberInfoLookup(entityType);
				if (idMemberInfo == null)
					throw new ConfigurationException("ID member lookup convention returned null for {0}", entityType);
				idMember = new PropertyOrPubilcFieldSpecialMember(entityType, idMemberInfo);
			}
			else
				idMember = null;

			var revisionMemberInfo = RevisionMemberInfoLookup == null? null: RevisionMemberInfoLookup(entityType);
			var revisionMember = revisionMemberInfo == null ? null : new PropertyOrPubilcFieldSpecialMember(entityType, revisionMemberInfo);

			return new EntityConfig(entityType, EntityTypeToDocumentType, idMember, revisionMember, DocumentIdToEntityId, EntityIdToDocumentId);
		}
	}
}
