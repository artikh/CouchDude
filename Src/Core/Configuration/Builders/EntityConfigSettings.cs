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
