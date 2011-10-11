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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Api
{
	public class CouchApiTests
	{
		[Fact]
		public void ShouldThrowOnNullEmptyOrWhitespaceDbName()
		{
			var couchApi = Factory.CreateCouchApi("http://example.com");
			Assert.Throws<ArgumentNullException>(() => couchApi.Db(null));
			Assert.Throws<ArgumentNullException>(() => couchApi.Db(string.Empty));
			Assert.Throws<ArgumentNullException>(() => couchApi.Db("   "));
		}

		[Theory]
		[InlineData("DB")]
		[InlineData("_db1")]
		[InlineData("$db1")]
		[InlineData("1db1")]
		[InlineData("(a)db1")]
		[InlineData(")b(db1")]
		[InlineData("[db1")]
		[InlineData("+db1")]
		[InlineData("-db1")]
		public void ShouldThrowOnInvalidDbName(string invalidDbName)
		{
			var couchApi = Factory.CreateCouchApi("http://example.com");
			var exception = Assert.Throws<ArgumentOutOfRangeException>(() => couchApi.Db(invalidDbName));
			Assert.Equal(invalidDbName, exception.ActualValue as string);
		}

		[Theory]
		[InlineData("adb")]
		[InlineData("a_db1")]
		[InlineData("a$db1")]
		[InlineData("a1db1")]
		[InlineData("a(a)db1")]
		[InlineData("a)b(db1")]
		[InlineData("a+db1")]
		[InlineData("a-db1")]
		public void ShouldNotThrowOnValidDbName(string validDbName)
		{
			var couchApi = Factory.CreateCouchApi("http://example.com");
			Assert.DoesNotThrow(() => couchApi.Db(validDbName));
		}
	}
}
