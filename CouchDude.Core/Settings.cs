using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
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

		private readonly ConcurrentDictionary<Type, string>
			docTypeMap = new ConcurrentDictionary<Type, string>();

		/// <summary>Base server URL.</summary>
		public readonly Uri ServerUri;

		/// <summary>Database name.</summary>
		public readonly string DatabaseName;

		/// <summary>Document id property convension.</summary>
		public ISpecialPropertyConvention IdPropertyConvention = 
			new PropertyByNameConvention("Id", "ID");

		/// <summary>Document revision property convension.</summary>
		public ISpecialPropertyConvention RevisionPropertyConvention = 
			new PropertyByNameConvention("Revision", "Rev");

		/// <summary>Document type detector convension.</summary>
		public IDocumentTypeConvention DocumentTypeConvension =
			new DocumentTypeFromClassNameConvention();

		/// <summary>Document ID generator.</summary>
		public IIdGenerator IdGenerator = new SequentialUuidIdGenerator();
		
		/// <constructor />
		public Settings(Uri serverUri, string databaseName)
		{
			if (serverUri == null)
				throw new ArgumentNullException("serverUri");
			if (!serverUri.IsAbsoluteUri)
				throw new ArgumentException("Server URL should be absolute.", "serverUri");
			if(string.IsNullOrWhiteSpace(databaseName))
				throw new ArgumentNullException("databaseName");
			if(!ValidDbName(databaseName))
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

		/// <summary>Returns document type for entity </summary>
		public string GetDocumentType<TEntity>() where TEntity: class
		{
			return docTypeMap.GetOrAdd(typeof (TEntity), t => DocumentTypeConvension.GetType(t));
		}
	}
}