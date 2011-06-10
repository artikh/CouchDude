using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using CouchDude.Core.Configuration;

namespace CouchDude.Core
{
	/// <summary>CouchDude settings.</summary>
	public class Settings
	{
		private readonly EntityRegistry entityRegistry = new EntityRegistry();
		private Uri serverUri;
		private string databaseName;

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
				Contract.EndContractBlock();

				serverUri = value;
			}
		}

		/// <summary>Database name.</summary>
		public string DatabaseName
		{
			get { return databaseName; }
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("value");
				if (!ValidDbName(value))
					throw new ArgumentException(
						"A database must be named with all lowercase letters (a-z), " +
							"digits (0-9), or any of the _$()+-/ characters and must end with a " +
							"slash in the URL. The name has to start with a lowercase letter (a-z).",
						"value");
				Contract.EndContractBlock();

				if(databaseName == value) return;
				databaseName = value;
			}
		}

		/// <summary>Document ID generator.</summary>
		public IIdGenerator IdGenerator = new SequentialUuidIdGenerator();

		/// <summary>Reports</summary>
		public bool Incomplete { get { return databaseName == null || serverUri == null; } }

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
			if (!ValidDbName(databaseName))
				throw new ArgumentException(
					"A database must be named with all lowercase letters (a-z), " +
					"digits (0-9), or any of the _$()+-/ characters and must end with a " +
					"slash in the URL. The name has to start with a lowercase letter (a-z).",
					"databaseName");
			Contract.EndContractBlock();

			ServerUri = serverUri;
			DatabaseName = databaseName;
		}

		/// <summary>Determines if provided database name is valid.</summary>
		[Pure]
		public static bool ValidDbName(string databaseName)
		{
			var firstLetter = databaseName[0];
			return Char.IsLetter(firstLetter)
			       && Char.IsLower(firstLetter)
			       && databaseName.All(ch => Regex.IsMatch(ch.ToString(), "[0-9a-z_$()+-/]"));
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
	}
}