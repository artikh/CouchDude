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
using System.Diagnostics.Contracts;
using System.Reflection;

namespace CouchDude.Core.Configuration
{
	/// <summary>Entity type to document type conversion convention.</summary>
	public delegate string EntityTypeToDocumentTypeConvention(Type entityType);

	/// <summary>Entity ID to document ID conversion convention.</summary>
	public delegate string EntityIdToDocumentIdConvention(string entityId, Type entityType, string documentType);

	/// <summary>Document ID to entity ID conversion convention.</summary>
	public delegate string DocumentIdToEntityIdConvention(string documentId, string documentType, Type entityType);

	/// <summary>Default entity configuration object delegating all actions to public field delegates.</summary>
	public class EntityConfig : IEntityConfig
	{
		private readonly ISpecialMember idMember;
		private readonly ISpecialMember revisionMember;
		private readonly DocumentIdToEntityIdConvention documentIdToEntityId;
		private readonly EntityIdToDocumentIdConvention entityIdToDocumentId;

		/// <constructor />
		public EntityConfig(
			Type entityType, 
			EntityTypeToDocumentTypeConvention entityTypeToDocumentType = null, 
			ISpecialMember idMember = null, 
			ISpecialMember revisionMember = null,
			DocumentIdToEntityIdConvention documentIdToEntityId = null,
			EntityIdToDocumentIdConvention entityIdToDocumentId = null)
		{
			if (entityType == null) throw new ArgumentNullException("entityType");
			Contract.EndContractBlock();

			idMember                    = idMember                  ?? DefaultEntityConfigConventions.GetIdMember(entityType);
			revisionMember              = revisionMember            ?? DefaultEntityConfigConventions.GetRevisionMember(entityType);
			entityTypeToDocumentType    = entityTypeToDocumentType  ?? DefaultEntityConfigConventions.EntityTypeToDocumentType;
			documentIdToEntityId        = documentIdToEntityId      ?? DefaultEntityConfigConventions.DocumentIdToEntityId;
			entityIdToDocumentId        = entityIdToDocumentId      ?? DefaultEntityConfigConventions.EntityIdToDocumentId;

			EntityType = entityType;
			DocumentType = entityTypeToDocumentType(entityType);
			this.idMember = idMember;
			this.revisionMember = revisionMember;
			this.documentIdToEntityId = documentIdToEntityId;
			this.entityIdToDocumentId = entityIdToDocumentId;
			IgnoredMembers = GetIgnoredMemberInfo(idMember, revisionMember);
		}

		private static IEnumerable<MemberInfo> GetIgnoredMemberInfo(ISpecialMember idMember, ISpecialMember revisionMember)
		{
			var ignoredMembers = new List<MemberInfo>(2);
			if (idMember.IsDefined && idMember.RawMemberInfo != null)
				ignoredMembers.Add(idMember.RawMemberInfo);
			if (revisionMember.IsDefined && revisionMember.RawMemberInfo != null)
				ignoredMembers.Add(revisionMember.RawMemberInfo);
			return ignoredMembers;
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

			return documentIdToEntityId(documentId, DocumentType, EntityType);
		}

		/// <inheritdoc/>
		public virtual string ConvertEntityIdToDocumentId(string entityId)
		{
			if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("entityId");
			Contract.EndContractBlock();

			return entityIdToDocumentId(entityId, EntityType, DocumentType);
		}

		/// <inheritdoc/>
		public bool IsIdMemberPresent { get { return idMember.IsDefined; } }

		/// <inheritdoc/>
		public virtual void SetId(object entity, string entityId)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException("entityId");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			idMember.SetValue(entity, entityId);
		}

		/// <inheritdoc/>
		public virtual string GetId(object entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			return idMember.GetValue(entity);
		}
		
		/// <inheritdoc/>
		public bool IsRevisionPresent { get { return revisionMember.IsDefined; } }

		/// <inheritdoc/>
		public virtual void SetRevision(object entity, string entityRevision)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (string.IsNullOrEmpty(entityRevision)) throw new ArgumentNullException("entityRevision");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			revisionMember.SetValue(entity, entityRevision);
		}

		/// <inheritdoc/>
		public virtual string GetRevision(object entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			if (!EntityType.IsAssignableFrom(entity.GetType()))
				throw new ArgumentException("Entity should be assignable to {0}.", EntityType.AssemblyQualifiedName);
			Contract.EndContractBlock();

			return revisionMember.GetValue(entity);
		}

		/// <inheritdoc/>
		public virtual IEnumerable<MemberInfo> IgnoredMembers { get; private set; }
	}
}