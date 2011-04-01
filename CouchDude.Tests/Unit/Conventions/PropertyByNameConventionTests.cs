using CouchDude.Core.Conventions;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class PropertyByNameConventionTests
	{
		private readonly ISpecialPropertyConvention convention = 
			new PropertyByNameConvention("ID", "Id");

		private class UppercaseIdPropertyClass
		{
			public string ID { get; set; }
		}

		private class PascalcaseIdPropertyClass
		{
			public string Id { get; set; }
		}

		private class PrivateSetterIdPropertyClass
		{
			public string Id { get; private set; }
		}

		[Fact]
		public void ShouldReturnNullIfNoIdProperty()
		{
			Assert.Null(convention.Get(typeof(object)));
		}

		[Fact]
		public void ShouldDetectUppercaseIdProperty()
		{
			const string sampleValue = "{CBE4A1F1-4AFB-4E46-BBFC-0B28455A7011}";

			var entity = new UppercaseIdPropertyClass();
			var property = convention.Get(entity.GetType());

			Assert.NotNull(property);
			property.SetIfAble(entity, sampleValue);
			Assert.Equal(sampleValue, property.GetIfAble(entity));
		}

		[Fact]
		public void ShouldDetectPascalcaseIdProperty()
		{
			const string sampleValue = "{04DA10E0-0E45-472F-9B26-1FCC3C8025BD}";

			var entity = new PascalcaseIdPropertyClass();
			var property = convention.Get(entity.GetType());

			Assert.NotNull(property);
			property.SetIfAble(entity, sampleValue);
			Assert.Equal(sampleValue, property.GetIfAble(entity));
		}

		[Fact]
		public void ShouldDetectPrivateSetterIdProperty()
		{
			const string sampleValue = "{04DA10E0-0E45-472F-9456-1FCC3C8025BD}";

			var entity = new PrivateSetterIdPropertyClass();
			var property = convention.Get(entity.GetType());

			Assert.NotNull(property);
			property.SetIfAble(entity, sampleValue);
			Assert.Equal(sampleValue, property.GetIfAble(entity));
		}
	}
}