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
using System.Threading;

namespace CouchDude.Configuration
{
	/// <summary>Simple wrapper over collection of <see cref="EntityConfig"/> instances with
	/// lookup by document type and entity type.</summary>
	internal class EntityRegistry
	{
		private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
		private readonly IDictionary<string, IEntityConfig> documentTypeMap = new Dictionary<string, IEntityConfig>();
		private readonly IDictionary<Type, IEntityConfig> entityTypeMap = new Dictionary<Type, IEntityConfig>();

		public void Register(IEntityConfig config)
		{
			readerWriterLock.EnterWriteLock();
			try
			{
				var documentType = config.DocumentType;
				var entityType = config.EntityType;
				if (entityTypeMap.ContainsKey(entityType))
					throw new ConfigurationException(
						"Duplicate registration of entity type {0}. You should revise your configuration code.", entityType);
				if(documentTypeMap.ContainsKey(documentType))
					throw new ConfigurationException(
						"Duplicate registration of document type '{0}' (was for {1} and now for {2}). " +
							"You should revise your document type conventions.",
						documentType,
						documentTypeMap[documentType].EntityType,
						entityType);

				documentTypeMap[documentType] = config;
				entityTypeMap[entityType] = config;
			}
			finally
			{
				readerWriterLock.ExitWriteLock();
			}
		}

		public IEntityConfig this[string documentType]
		{
			get
			{
				readerWriterLock.EnterReadLock();
				try
				{
					IEntityConfig entityConfig;
					if(!documentTypeMap.TryGetValue(documentType, out entityConfig))
						throw new DocumentTypeNotRegistredException(documentType);
					return entityConfig;
				}
				finally
				{
					readerWriterLock.ExitReadLock();
				}
			}
		}

		public IEntityConfig this[Type entityType]
		{
			get
			{
				readerWriterLock.EnterReadLock();
				try
				{
					IEntityConfig entityConfig;
					if (!entityTypeMap.TryGetValue(entityType, out entityConfig))
						throw new EntityTypeNotRegistredException(entityType);
					return entityConfig;
				}
				finally
				{
					readerWriterLock.ExitReadLock();
				}
			}
		}

		public bool Contains(string documentType)
		{
			readerWriterLock.EnterReadLock();
			try
			{
				return documentTypeMap.ContainsKey(documentType);
			}
			finally
			{
				readerWriterLock.ExitReadLock();
			}
		}

		public bool Contains(Type entityType)
		{
			readerWriterLock.EnterReadLock();
			try
			{
				return entityTypeMap.ContainsKey(entityType);
			}
			finally
			{
				readerWriterLock.ExitReadLock();
			}
		}
	}
}
