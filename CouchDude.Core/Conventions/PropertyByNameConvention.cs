using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CouchDude.Core.Conventions
{
	/// <summary>Searhes for document ID entity property.</summary>
	public class PropertyByNameConvention: ISpecialPropertyConvention
	{
		private readonly string[] propertyNames;

		/// <constructor />
		public PropertyByNameConvention(params string[] propertyNames)
		{
			this.propertyNames = propertyNames;
		}

		/// <inheritdoc/>
		public SpecialPropertyDescriptor Get(Type entityType)
		{
			var findProperty = FindProperty(entityType).FirstOrDefault(p => p != null);
			if (findProperty == null)
				return null;

			Action<object, string> setter = null;
			Func<object, string> getter = null;

			if(findProperty.CanRead)
			{
				var getMethod = findProperty.GetGetMethod(true);
				getter = instance => (string)getMethod.Invoke(instance, new object[0]);
			}

			if(findProperty.CanWrite)
			{
				var setMethod = findProperty.GetSetMethod(true);
				setter = (instance, value) => setMethod.Invoke(instance, new[] { value });
			}

			return new SpecialPropertyDescriptor(setter, getter);
		}

		private IEnumerable<PropertyInfo> FindProperty(Type entityType)
		{
			const BindingFlags bindingFlags = 
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			
			return propertyNames.Select(pn => entityType.GetProperty(pn, bindingFlags));
		}
	}
}