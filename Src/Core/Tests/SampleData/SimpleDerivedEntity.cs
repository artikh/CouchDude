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

namespace CouchDude.Tests.SampleData
{
	public class SimpleDerivedEntity : SimpleEntity
	{
		public new const string StandardId = "doc1";
		public new const string StandardRevision = "1-cc2c5ab22cfa4a0faad27a0cb9ca7968";
		public new const string DocType = "simpleEntity";
		public new const string StandardDocId = DocType + ".doc1";

		public new static SimpleDerivedEntity CreateStandard() { return new SimpleDerivedEntity { Id = StandardId, Revision = StandardRevision, Age = 42 }; }

		public new static SimpleDerivedEntity CreateStandardWithoutRevision() { return new SimpleDerivedEntity { Id = StandardId, Age = 42 }; }

		public new static IDocument CreateDocument() { return new { _id = StandardDocId, _rev = StandardRevision, type = DocType, age = 42 }.ToDocument(); }

		public new static IDocument CreateDocumentWithoutRevision() { return new { _id = StandardDocId, type = DocType, age = 42 }.ToDocument(); }
	}
}