using System;

namespace CouchDude.Core.Conventions
{
	/// <summary>Handles spectial properties operations.</summary>
	public class SpecialPropertyDescriptor
	{
		private readonly Action<object, string> setter;
		private readonly Func<object, string> getter;

		/// <summary>Doing nothing descriptor.</summary>
		public static SpecialPropertyDescriptor Noop =
			new SpecialPropertyDescriptor(null, null);

		/// <constructor />
		public SpecialPropertyDescriptor(Action<object, string> setter, Func<object, string> getter)
		{
			this.setter = setter;
			this.getter = getter;
		}

		/// <summary>Returns if property colud be read.</summary>
		public bool CanRead { get { return getter != null; } }

		/// <summary>Sets special propertie's new value.</summary>
		public void SetIfAble(object entityInstance, string value)
		{
			if (setter != null) 
				setter(entityInstance, value);
		}

		/// <summary>Gets special propertie's current value.</summary>
		public string GetIfAble(object entityInstance)
		{
			return getter != null ? getter(entityInstance) : null;
		}
	}
}