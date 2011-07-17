#region Licence Info 
/*
  Copyright 2011 · Artem Tikhomirov																					
 																																					
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

using CouchDude.Core;
using CouchDude.Core.Utils;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Utils
{
	public class JsonUtilsTests
	{
		[Fact]
		public void ShouldParseDocumentInfo()
		{
			var obj = JObject.Parse(@"{ ""some_prop"": ""some value"" }");
			Assert.Equal("some value", obj.GetRequiredProperty("some_prop"));
		}

		[Fact]
		public void ShouldThrowOnMissigProperty()
		{
			var obj = JObject.Parse(@"{  }");
            Assert.Throws<ParseException>(() =>
				obj.GetRequiredProperty("some_prop")
			);
		}

		[Fact]
		public void ShouldThrowOnEmptyProperty()
		{
			var obj = JObject.Parse(@"{  ""some_prop"": """"  }");
            Assert.Throws<ParseException>(() =>
				obj.GetRequiredProperty("some_prop")
			);
		}

		[Fact]
		public void ShouldThrowOnWhitespaseProperty()
		{
			var obj = JObject.Parse(@"{  ""some_prop"": ""   ""  }");
            Assert.Throws<ParseException>(() =>
				obj.GetRequiredProperty("some_prop")
			);
		}
	}
}
