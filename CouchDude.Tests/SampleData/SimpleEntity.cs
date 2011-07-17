using System;
using CouchDude.Core;
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

		public static JObject DocumentWithRevision = new {
				_id = StandardDocId,
				_rev = StandardRevision,
				type = "simpleEntity",
				name = "John Smith",
				age = 42,
				date = "1957-04-10T00:00:00"
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