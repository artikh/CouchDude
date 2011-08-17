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

using System;

using CouchDude.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Tests.SampleData
{
	public class SimpleEntity : IEntity
	{
		public const string StandardRevision = "1-1a517022a0c2d4814d51abfedf9bfee7";
		public const string StandardEntityId = "doc1";
		public const string StandardDocId = "simpleEntity.doc1";

		public static JObject OkResponse = new {
				ok = true,
				id = StandardDocId,
				rev = StandardRevision
			}.ToJObject();

		public static Document DocWithRevision = new Document(new {
				_id = StandardDocId,
				_rev = StandardRevision,
				type = "simpleEntity",
				name = "John Smith",
				age = 42,
				date = "1957-04-10T00:00:00"
			}.ToJsonTextReader());

		public static SimpleEntity CreateStd()
		{
			return new SimpleEntity
			       	{
			       		Id = StandardEntityId,
			       		Revision = StandardRevision,
			       		Name = "John Smith",
			       		Age = 42,
			       		Date = new DateTime(1957, 4, 10)
			       	};
		}

		public static SimpleEntity CreateStdWithoutRevision()
		{
			return new SimpleEntity
			       	{
			       		Id = StandardEntityId,
			       		Name = "John Smith",
			       		Age = 42,
			       		Date = new DateTime(1957, 4, 10)
			       	};
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