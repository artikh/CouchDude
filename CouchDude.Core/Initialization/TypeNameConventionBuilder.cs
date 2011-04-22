using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using CouchDude.Core.Conventions;

namespace CouchDude.Core.Initialization
{
	/// <summary>Provides fluent interface for setting type convention parameters.</summary>
	public class TypeNameConventionBuilder
	{
		private readonly ISet<Assembly> assemblies = new HashSet<Assembly>();
		private readonly List<Type> baseTypes = new List<Type>();

		private readonly SessionFactoryBuilder sessionFactoryBuilder;

		/// <constructor />
		public TypeNameConventionBuilder(SessionFactoryBuilder sessionFactoryBuilder)
		{
			this.sessionFactoryBuilder = sessionFactoryBuilder;
		}

		/// <summary>Adds type's assembly to the list of assemblies to scan for entities.</summary>
		public TypeNameConventionBuilder FromAssemblyOf<T>()
		{
			assemblies.Add(typeof (T).Assembly);
			return this;
		}

		/// <summary>Adds assembly to the list of assemblies to scan for entities.</summary>
		public TypeNameConventionBuilder FromAssembly(Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");
			Contract.EndContractBlock();

			assemblies.Add(assembly);
			return this;
		}

		/// <summary>Adds assembly to the list of assemblies to scan for entities.</summary>
		public TypeNameConventionBuilder FromAssembly(string assemblyName)
		{
			if (assemblyName == null) throw new ArgumentNullException("assemblyName");
			Contract.EndContractBlock();

			assemblies.Add(Assembly.Load(assemblyName));
			return this;
		}

		/// <summary>Requires entity types to inherit from provided base class.</summary>
		public TypeNameConventionBuilder InheritingFrom<T>()
		{
			return InheritingFrom(typeof (T));
		}

		/// <summary>Requires entity types to inherit from provided base class.</summary>
		public TypeNameConventionBuilder InheritingFrom(Type baseClass)
		{
			if (baseClass == null) throw new ArgumentNullException("baseClass");
			if (baseClass.IsInterface) throw new ArgumentException("Base class should not be an interface.", "baseClass");
			Contract.EndContractBlock();

			baseTypes.Add(baseClass);
			return this;
		}

		/// <summary>Requires entity types to implement given interface.</summary>
		public TypeNameConventionBuilder Imprementing<T>()
		{
			return Imprementing(typeof(T));
		}

		/// <summary>Requires entity types to implement given interface.</summary>
		public TypeNameConventionBuilder Imprementing(Type @interface)
		{
			if (@interface == null) throw new ArgumentNullException("interface");
			if (!@interface.IsInterface) throw new ArgumentException("Type should be an interface.", "interface");
			Contract.EndContractBlock();

			baseTypes.Add(@interface);
			return this;
		}

		/// <summary>Sets camelCase entity type convention.</summary>
		public SessionFactoryBuilder ToDocumentTypeCamelCase()
		{
			return ApplyConvention(new CamelCaseTypeNameToConvention(assemblies, baseTypes));
		}

		/// <summary>Sets PascalCase entity type convention.</summary>
		public SessionFactoryBuilder ToDocumentTypePascalCase()
		{
			return ApplyConvention(new TypeNameAsIsTypeConvention(assemblies, baseTypes));
		}

		/// <summary>Sets PascalCase entity type convention.</summary>
		public SessionFactoryBuilder ToDocumentTypeAs(Func<Type, string> createDocumentTypeFromEntityType)
		{
			return ApplyConvention(new CustomTypeConvention(assemblies, baseTypes, createDocumentTypeFromEntityType));
		}

		private SessionFactoryBuilder ApplyConvention(TypeConventionBase convention)
		{
			sessionFactoryBuilder.Settings.TypeConvension = convention;
			convention.Init();
			return sessionFactoryBuilder;
		}
	}
}