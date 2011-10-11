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

using CouchDude.SchemeManager;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.SchemeManager
{
	public class DesignDocumentTests
	{
		[Fact]
		public void ShouldCopyWithRevisionProperly()
		{
			var json = JObject.Parse(@"{
				""_id"": ""_design/bin_doc1"",
				""some_property1"": ""test content""
			}");

			var document = new DesignDocument(json, "_design/bin_doc1", null);
			var copiedDocument = document.CopyWithRevision("3-ee7084f94345720bf9fdcd8f087e5518");

			Assert.Equal("_design/bin_doc1", copiedDocument.Id);
			Assert.Equal("3-ee7084f94345720bf9fdcd8f087e5518", copiedDocument.Revision);
			Assert.Equal(
				JObject.Parse(@"{
					""_id"": ""_design/bin_doc1"",
					""_rev"": ""3-ee7084f94345720bf9fdcd8f087e5518"",
					""some_property1"": ""test content""
				}"),
				copiedDocument.Definition,
				new JTokenEqualityComparer()
			);
		}
	}
}
