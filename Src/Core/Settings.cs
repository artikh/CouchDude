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
using System.Collections.Generic;
using CouchDude.Configuration;
using CouchDude.Utils;

namespace CouchDude
{
	/// <summary>CouchDude settings.</summary>
	public class Settings : IEntityConfigRepository
	{
		private readonly EntityRegistry entityRegistry = new EntityRegistry();
		private Uri serverUri;
		private string defaultDatabaseName;

		/// <summary>Base server URL.</summary>
		public Uri ServerUri
		{
			get { return serverUri; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				if (!value.IsAbsoluteUri)
					throw new ArgumentException("Server URL should be absolute.", "value");
				

				serverUri = value;
			}
		}

		/// <summary>Database name.</summary>
		public string DefaultDatabaseName
		{
			get { return defaultDatabaseName; }
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("value");
				CheckIf.DatabaseNameIsOk(value, "value");
				

				if (defaultDatabaseName == value) return;
				defaultDatabaseName = value;
			}
		}

		/// <summary>Document ID generator.</summary>
		public IIdGenerator IdGenerator = new SequentialUuidIdGenerator();

		/// <summary>Reports</summary>
		public bool Incomplete
		{
			get { return defaultDatabaseName == null || serverUri == null; }
		}

		/// <constructor />
		public Settings() { }

		/// <constructor />
		public Settings(Uri serverUri, string databaseName)
		{
			if (serverUri == null)
				throw new ArgumentNullException("serverUri");
			if (!serverUri.IsAbsoluteUri)
				throw new ArgumentException("Server URL should be absolute.", "serverUri");
			if (string.IsNullOrWhiteSpace(databaseName))
				throw new ArgumentNullException("databaseName");
			CheckIf.DatabaseNameIsOk(databaseName, "databaseName");
			

			ServerUri = serverUri;
			DefaultDatabaseName = databaseName;
		}

		/// <summary>Registers entity configuration.</summary>
		public Settings Register(IEntityConfig entityConfig)
		{
			entityRegistry.Register(entityConfig);
			return this;
		}
		
		/// <summary>Retrives entity configuration by entity type.</summary>
		public IEntityConfig GetConfig(Type entityType)
		{
			return entityRegistry[entityType];
		}

		/// <summary>Retrives entity configuration by entity type returning <c>null</c> if none found.</summary>
		public IEntityConfig TryGetConfig(Type entityType)
		{
			return entityRegistry.Contains(entityType) ? entityRegistry[entityType] : null;
		}

		/// <summary>Retrives entity configuration by document type.</summary>
		public IEntityConfig GetConfig(string documentType)
		{
			return entityRegistry[documentType];
		}

		/// <inheritdoc />
		public IEnumerable<Type> GetAllRegistredBaseTypes(Type entityType)
		{
			if (entityType == null) 
				yield break;

			var currentType = entityType;

			while (currentType != null && entityRegistry.Contains(currentType))
			{
				yield return currentType;
				currentType = currentType.BaseType;
			}
		}
	}
}
