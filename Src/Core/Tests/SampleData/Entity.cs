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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Tests.SampleData
{
	public class Entity : IEntity
	{
		public const string StandardRevision = "1-1a517022a0c2d4814d51abfedf9bfee7";
		public const string StandardEntityId = "doc1";
		public const string StandardDocId = DocType + ".doc1";
		public const string DocType = "entity";

		public static JObject OkResponse = new {
				ok = true,
				id = StandardDocId,
				rev = StandardRevision
			}.ToJObject();

		public readonly static DocumentInfo StandardDococumentInfo = new DocumentInfo(StandardDocId, StandardRevision);

		public static Entity CreateStandard()
		{
			return new Entity
			       	{
			       		Id = StandardEntityId,
			       		Revision = StandardRevision,
			       		Name = "John Smith",
			       		Age = 42,
			       		Date = new DateTime(1957, 4, 10)
			       	};
		}

		public static Entity CreateStandardWithoutRevision()
		{
			return new Entity
			       	{
			       		Id = StandardEntityId,
			       		Name = "John Smith",
			       		Age = 42,
			       		Date = new DateTime(1957, 4, 10)
			       	};
		}

		public static IDocument CreateDocWithRevision()
		{
			return new Document(
				new {
					_id = StandardDocId,
					_rev = StandardRevision,
					type = DocType,
					name = "John Smith",
					age = 42,
					date = "1957-04-10T00:00:00Z"
				}.ToJsonTextReader());
		}

		public static IDocument CreateDocWithoutRevision()
		{
			return new Document(
				new {
					_id = StandardDocId,
					type = DocType,
					name = "John Smith",
					age = 42,
					date = "1957-04-10T00:00:00Z"
				}.ToJsonTextReader());
		}

		[JsonIgnore]
		public string Id { get; set; }

		[JsonIgnore]
		public string Revision { get; set; }

		public string Name { get; set; }
		public int Age { get; set; }
		public DateTime Date { get; set; }

		public void DoStuff() { }
	}
}