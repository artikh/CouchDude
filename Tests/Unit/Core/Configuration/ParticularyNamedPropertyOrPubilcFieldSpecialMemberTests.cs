#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

using System;
using CouchDude.Configuration;
using Xunit;
using Xunit.Extensions;
#pragma warning disable 169

namespace CouchDude.Tests.Unit.Core.Configuration
{
	public class ParticularyNamedPropertyOrPubilcFieldSpecialMemberTests
	{
		// ReSharper disable InconsistentNaming
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		// ReSharper disable ValueParameterNotUsed
		// ReSharper disable UnusedMember.Local
		public class PrivatePropertyEntity { private string Name { get; set; } }
		public class PublicPropertyEntity { public string Name { get; set; } }
		public class PublicFieldEntity { public string Name; }

		public class PrivateFieldEntity { private string Name; }
		public class PropertyWithoutSetterEntity { public string Name { get { return null; } } }
		public class PropertyWithoutGetterEntity { public string Name { set { } } }
		public class EmptyEntity { }
		public class MethodEntity { void Name() {} }

		// ReSharper restore UnusedMember.Local
		// ReSharper restore ValueParameterNotUsed
		// ReSharper restore UnusedAutoPropertyAccessor.Local
		// ReSharper restore InconsistentNaming	

		[Theory]
		[InlineData(typeof(PrivatePropertyEntity))]
		[InlineData(typeof(PublicPropertyEntity))]
		[InlineData(typeof(PublicFieldEntity))]
		public void ShouldSetAndGetSpecialProperty(Type type)
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(type, "Name");
			var instance = Activator.CreateInstance(type);
			descriptor.SetValue(instance, "John");
			Assert.Equal("John", descriptor.GetValue(instance));
		}

		[Theory]
		[InlineData(typeof(PrivatePropertyEntity))]
		[InlineData(typeof(PublicPropertyEntity))]
		[InlineData(typeof(PublicFieldEntity))]
		public void ShouldDetectIfMemberPresent(Type type)
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(type, "Name");
			Assert.True(descriptor.IsDefined);
		}

		[Theory]
		[InlineData(typeof(PrivatePropertyEntity))]
		[InlineData(typeof(PublicPropertyEntity))]
		[InlineData(typeof(PublicFieldEntity))]
		public void ShouldReturnMemberInfoIfDetected(Type type)
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(type, "Name");
			Assert.NotNull(descriptor.RawMemberInfo);
		}

		[Theory]
		[InlineData(typeof(PrivateFieldEntity))]
		[InlineData(typeof(PropertyWithoutSetterEntity))]
		[InlineData(typeof(PropertyWithoutGetterEntity))]
		[InlineData(typeof(EmptyEntity))]
		public void ShouldReturnNullIfNotDetected(Type type)
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(type, "Name");
			Assert.Null(descriptor.RawMemberInfo);
		}

		[Fact]
		public void ShouldIgnoreNonePropertiesOrFields()
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(typeof(MethodEntity), "Name");
			Assert.False(descriptor.IsDefined);
		}

		[Fact]
		public void ShouldIgnorePropertiesWithoutGetter()
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(typeof(PropertyWithoutGetterEntity), "Name");
			Assert.False(descriptor.IsDefined);
		}

		[Fact]
		public void ShouldIgnorePropertiesWithoutSetter()
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(typeof(PropertyWithoutSetterEntity), "Name");
			Assert.False(descriptor.IsDefined);
		}

		[Fact]
		public void ShouldIgnoreFieldIfItIsPrivate()
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(typeof(PrivateFieldEntity), "Name");
			Assert.False(descriptor.IsDefined);
		}

		[Fact]
		public void ShouldDetectIfNoPropertyOrPubilcFieldDetected()
		{
			var descriptor = new ParticularyNamedPropertyOrPubilcFieldSpecialMember(typeof(EmptyEntity), "Name");
			Assert.False(descriptor.IsDefined);
		}
	}
}