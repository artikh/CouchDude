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
using System.Collections.Generic;

using System.Linq;
using System.Reflection;

namespace CouchDude.Configuration.Builders
{
	/// <summary>Entity configuration class builder.</summary>
	public class EntityConfigBuilder<TSelf>: SettingsBuilderDetour where TSelf: EntityConfigBuilder<TSelf>
	{
		/// <summary>Entities to attach config to restrictions.</summary>
		protected IList<Predicate<Type>> Predicates = new List<Predicate<Type>>();
		/// <summary>Assemblies to be scaned in search of entities to assign build confg to.</summary>
		protected ISet<Assembly> AssembliesToScan = new HashSet<Assembly>();

		private EntityTypeToDocumentTypeConvention entityTypeToDocumentType;
		private Func<Type, MemberInfo> idMemberInfoLookup;
		private Func<Type, MemberInfo> revisionMemberInfoLookup = _ => null;
		private DocumentIdToEntityIdConvention documentIdToEntityId;
		private EntityIdToDocumentIdConvention entityIdToDocumentId;

		/// <constructor />
		public EntityConfigBuilder(SettingsBuilder parent): base(parent) { }

		/// <summary>Sets entity type to document type convention.</summary>
		public TSelf WhenDocumentType(EntityTypeToDocumentTypeConvention entityTypeToDocumentType)
		{
			this.entityTypeToDocumentType = entityTypeToDocumentType;
			return (TSelf) this;
		}

		/// <summary>Sets entity type to document type convention.</summary>
		public TSelf TranslatingDocumentIdToEntityIdAs(DocumentIdToEntityIdConvention documentIdToEntityId)
		{
			this.documentIdToEntityId = documentIdToEntityId;
			return (TSelf) this;
		}

		/// <summary>Sets entity type to document type convention.</summary>
		public TSelf TranslatingEntityIdToDocumentIdAs(EntityIdToDocumentIdConvention entityIdToDocumentId)
		{
			this.entityIdToDocumentId = entityIdToDocumentId;
			return (TSelf) this;
		}

		/// <summary>Sets entity type to document type convention.</summary>
		public TSelf WhenIdMember(Func<Type, MemberInfo> idMemberInfoLookup)
		{
			if (idMemberInfoLookup == null) throw new ArgumentNullException("idMemberInfoLookup");
			

			this.idMemberInfoLookup = idMemberInfoLookup;
			return (TSelf) this;
		}

		/// <summary>Sets entity type to document type convention.</summary>
		public TSelf WhenRevisionMember(Func<Type, MemberInfo> revisionMemberInfoLookup)
		{
			if (revisionMemberInfoLookup == null) throw new ArgumentNullException("revisionMemberInfoLookup");
			

			this.revisionMemberInfoLookup = revisionMemberInfoLookup;
			return (TSelf) this;
		}

		/// <inheritdoc/>
		protected override void Flush()
		{
			RegisterNewScanDescriptor(CreateEntityConfig);
		}

		private IEntityConfig CreateEntityConfig(Type entityType)
		{
			PropertyOrPubilcFieldSpecialMember idMember;
			if (idMemberInfoLookup != null)
			{
				var idMemberInfo = idMemberInfoLookup(entityType);
				if (idMemberInfo == null)
					throw new ConfigurationException("ID member lookup convention returned null for {0}", entityType);
				idMember = new PropertyOrPubilcFieldSpecialMember(entityType, idMemberInfo);
			}
			else
				idMember = null;

			var revisionMemberInfo = revisionMemberInfoLookup(entityType);
			var revisionMember = revisionMemberInfo == null? null: new PropertyOrPubilcFieldSpecialMember(entityType, revisionMemberInfo);

			return new EntityConfig(entityType, entityTypeToDocumentType, idMember, revisionMember, documentIdToEntityId, entityIdToDocumentId);
		}

		/// <summary>Saves current state as <see cref="ScanDescriptor"/> to parent builder.</summary>
		protected void RegisterNewScanDescriptor(Func<Type, IEntityConfig> configFactory)
		{
			var scanDescriptor = new ScanDescriptor(
				type => Predicates.All(p => p(type)), configFactory);
			foreach (var assembly in AssembliesToScan)
				Parent.RegisterScanDescriptor(assembly, scanDescriptor);
		}
	}
}