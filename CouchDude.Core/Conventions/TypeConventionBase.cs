using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CouchDude.Core.Conventions
{
	/// <summary>Type convention base class providing basic assembly scan and caching 
	/// functionality.</summary>
	public abstract class TypeConventionBase: ITypeConvention
	{
		private readonly IDictionary<Type, string> entity2DocMap = new Dictionary<Type, string>();
		private readonly IDictionary<string, Type> doc2EntityMap = new Dictionary<string, Type>();
		private readonly IEnumerable<Assembly> assembliesToScan;
		/// <summary>Base type and/or interfaces to be requried for entity type.</summary>
		private readonly ICollection<Type> baseTypes;

		/// <constructor />
		protected TypeConventionBase(IEnumerable<Assembly> assembliesToScan, ICollection<Type> baseTypes = null)
		{
			this.assembliesToScan = assembliesToScan ?? new Assembly[0];
			this.baseTypes = baseTypes;
		}

		/// <constructor />
		protected TypeConventionBase(params Assembly[] assembliesToScan) : this((IEnumerable<Assembly>)assembliesToScan) { }

		/// <summary>Initializes convention.</summary>
		/// <remarks>Run it before use.</remarks>
		public void Init()
		{
			var types = assembliesToScan.SelectMany(a => a.GetTypes()).ToArray();
			foreach (var type in types)
			{
				var documentType = ProcessType(type);
				if (documentType != null)
				{
					entity2DocMap.Add(type, documentType);
					doc2EntityMap.Add(documentType, type);
				}
			}
		}

		/// <summary>Returns document type for entity type if it's one.</summary>
		protected virtual string ProcessType(Type entityType)
		{
			if (baseTypes != null && baseTypes.Count > 0)
				foreach (var expectedBaseType in baseTypes)
					if (!expectedBaseType.IsAssignableFrom(entityType))
						return null;

			var documentType = CreateDocumentTypeFromEntityType(entityType);

			if (documentType != null)
			{
				var previouslyRegistredEntityType = GetEntityType(documentType);
				if (previouslyRegistredEntityType != null)
					throw new ConventionException(
						"Document type '{0}' could not be registred for entity {1}: it has been registred for entity {2} already.",
						documentType,
						entityType,
						previouslyRegistredEntityType);
			}

			return documentType;
		}
		
		/// <inheritdoc/>
		public string GetDocumentType(Type entityType)
		{
			string documentType;
			return !entity2DocMap.TryGetValue(entityType, out documentType)? null: documentType;
		}

		/// <inheritdoc/>
		public Type GetEntityType(string documentType)
		{
			Type entityType;
			return !doc2EntityMap.TryGetValue(documentType, out entityType) ? null : entityType;
		}

		/// <summary>Maps entity type to document type.</summary>
		protected internal abstract string CreateDocumentTypeFromEntityType(Type entityType);
	}
}