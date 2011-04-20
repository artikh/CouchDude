using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private readonly Assembly[] assembliesToScan;

		// ReSharper disable DoNotCallOverridableMethodsInConstructor
		/// <constructor />
		protected TypeConventionBase(params Assembly[] assembliesToScan)
		{
			this.assembliesToScan = assembliesToScan ?? new Assembly[0];
		}

		/// <summary>Initializes convention.</summary>
		/// <remarks>Run it before use.</remarks>
		public void Init()
		{
			foreach (var entityType in assembliesToScan.SelectMany(a => a.GetTypes()))
			{
				var documentType = ProcessType(entityType);
				if (documentType != null)
				{
					entity2DocMap.Add(entityType, documentType);
					doc2EntityMap.Add(documentType, entityType);
				}
			}
		}

		/// <summary>Returns document type for entity type if it's one.</summary>
		protected internal abstract string ProcessType(Type type);
		
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
	}
}