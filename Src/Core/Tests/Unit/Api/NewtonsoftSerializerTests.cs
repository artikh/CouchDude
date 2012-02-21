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
using CouchDude.Api;
using CouchDude.Serialization;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class NewtonsoftSerializerTests
	{
		[Fact]
		public void ShouldThrowOnNullArgumentToSerialize()
		{
			Assert.Throws<ArgumentNullException>(() => NewtonsoftSerializer.SerializeAsJsonObject(null));
		}

		[Fact]
		public void ShouldSerializeEnumsAsString()
		{
			var entity = new JsonObjectTests.User(sex: JsonObjectTests.UserSex.Female);
			dynamic fragment = NewtonsoftSerializer.SerializeAsJsonObject(entity);
			Assert.Equal("Female", (string)fragment.sex);
		}

		[Fact]
		public void ShouldSerializeDatesAccodingToIso8601()
		{
			var entity = new JsonObjectTests.User(timestamp: new DateTime(2011, 06, 01, 12, 04, 34, 444, DateTimeKind.Utc));
			dynamic fragment = NewtonsoftSerializer.SerializeAsJsonObject(entity);
			Assert.Equal("2011-06-01T12:04:34.444Z", (string)fragment.timestamp);
		}

		[Fact]
		public void ShouldConvertPropertyNameToCamelCase()
		{
			var entity = new JsonObjectTests.User(name: "john");
			dynamic fragment = NewtonsoftSerializer.SerializeAsJsonObject(entity);
			Assert.NotNull((string)fragment.name);
		}

		[Fact]
		public void ShouldSerializePublicFields()
		{
			var entity = new JsonObjectTests.User { Field = "quantum mechanics" };
			dynamic fragment = NewtonsoftSerializer.SerializeAsJsonObject(entity);
			Assert.Equal("quantum mechanics", (string)fragment.field);
		}
		
	}
}