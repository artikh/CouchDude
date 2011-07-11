using System;
using System.Reflection;

namespace CouchDude.Core.Configuration
{
	/// <summary>Abstracts special entity type member manipulations.</summary>
	public interface ISpecialMember
	{
		/// <summary>Determines if member is indeed defined.</summary>
		bool IsDefined { get; }

		/// <summary>Member's entity type.</summary>
		Type EntityType { get; }

		/// <summary>Sets new special member value. Does nothing if <see cref="IsDefined"/> is <c>false</c>.</summary>
		void SetValue(object instance, string value);

		/// <summary>Returns current value of special member. 
		/// Returns <c>null</c> if <see cref="IsDefined"/> is <c>false</c>.</summary>
		string GetValue(object instance);

		/// <summary>Returns raw <see cref="MemberInfo"/> instance if special member indeed implemented as
		/// class member. This is used for the purpures of ignoring this class member on the stage of serialisation.</summary>
		MemberInfo RawMemberInfo { get; }
	}
}