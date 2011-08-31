using System;
using Xunit;

namespace CouchDude.Tests
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class IntegrationTestAttribute : TraitAttribute
	{
		public IntegrationTestAttribute() : base("level", "integration") { }
	}
}
