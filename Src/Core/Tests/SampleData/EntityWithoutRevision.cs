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

using Newtonsoft.Json;

namespace CouchDude.Tests.SampleData
{
	public class EntityWithoutRevision: IEntity
	{
		public const string StandardRevision = "1-1a517022a0c2d4814d51abfedf9bfee7";
		public const string StandardEntityId = "doc1";
		public const string StandardDocId = DocType + ".doc1";
		public const string DocType = "entityWithoutRevision";

		public static DocumentInfo OkResponse = new DocumentInfo(StandardDocId, StandardRevision);

		public static EntityWithoutRevision CreateStandard()
		{
			return new EntityWithoutRevision
				{
					Id = StandardEntityId,
					Name = "John Smith",
					Age = 42,
					Date = new DateTime(1957, 4, 10)
				};
		}

		public static IDocument CreateDocumentWithRevision()
		{
			return new {
				_id = StandardDocId,
				_rev = StandardRevision,
				type = DocType,
				name = "John Smith",
				age = 42,
				date = "1959-04-10T00:00:00Z"
			}.ToDocument();
		}

		[JsonIgnore]
		public string Id { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }

		public DateTime Date { get; set; }
	}
}