using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using CouchDude.Core.Configuration;
using CouchDude.Core.Conventions;

namespace CouchDude.Core
{
	/// <summary>CouchDude settings.</summary>
	public class Settings
	{
		private readonly ConcurrentDictionary<Type, SpecialPropertyDescriptor>
			idPropertyDescriptorMap = new ConcurrentDictionary<Type, SpecialPropertyDescriptor>();

		private readonly ConcurrentDictionary<Type, SpecialPropertyDescriptor>
			revPropertyDescriptorMap = new ConcurrentDictionary<Type, SpecialPropertyDescriptor>();
		
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

		/// <summary>Document id property convension.</summary>
		public IIdPropertyConvention IdPropertyConvention = 
			new PropertyByNameConvention("Id", "ID");

		/// <summary>Document revision property convension.</summary>
		public IRevisionPropertyConvention RevisionPropertyConvention = new PropertyByNameConvention("Revision", "Rev");

		/// <summary>Document type detector convension.</summary>
		public ITypeConvention TypeConvension = new EmptyTypeConvention();

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
			       && databaseName.All(ch => Regex.IsMatch(ch.ToString(), "[0-9a-z_$()+-/]")
			          	);
		}
		
		/// <summary>Returns ID property descriptor.</summary>
		public SpecialPropertyDescriptor GetIdPropertyDescriptor<TEntity>() where TEntity: class
		{
			return idPropertyDescriptorMap.GetOrAdd(
				typeof (TEntity), 
				t => {
				     	var idPropertyDescriptor = IdPropertyConvention.Get(t);
				     	if (idPropertyDescriptor == null || !idPropertyDescriptor.CanRead)
				     		throw new ConventionException(
				     			"Convention have not found any readable ID property on {0} entity.", 
				     			typeof(TEntity).FullName);
				     	return idPropertyDescriptor;
				});
		}

		/// <summary>Returns revision property descriptor for given entity type.</summary>
		public SpecialPropertyDescriptor GetRevPropertyDescriptor<TEntity>() where TEntity: class
		{
			return GetRevPropertyDescriptor(typeof(TEntity));
		}

		/// <summary>Returns revision property descriptor for given entity type.</summary>
		public SpecialPropertyDescriptor GetRevPropertyDescriptor(Type type)
		{
			return revPropertyDescriptorMap.GetOrAdd(
				type, t => RevisionPropertyConvention.Get(t) ?? SpecialPropertyDescriptor.Noop);
		}

		/// <summary></summary>
		public IEntityConfig GetConfig<TEntityType>(TEntityType entity)
		{
			return null;
		}

		/// <summary></summary>
		public IEntityConfig GetConfigFromDocumentType(string documentType)
		{
			return null;
		}

	}
}