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

		// ReSharper disable DoNotCallOverridableMethodsInConstructor
		protected TypeConventionBase(params Assembly[] assembliesToScan)
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
		// ReSharper restore DoNotCallOverridableMethodsInConstructor

		/// <summary>Returns document type for entity type if it's one.</summary>
		protected abstract string ProcessType(Type type);

		/// <summary>Checks if document type have been registered.</summary>
		protected bool HaveRegistered(string documentType)
		{
			return doc2EntityMap.ContainsKey(documentType);
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
	}

	/// <summary></summary>
	public class TypeNameToCamelCaseConvention: TypeConventionBase
	{
		protected override string ProcessType(Type type)
		{
			
		}
	}
}