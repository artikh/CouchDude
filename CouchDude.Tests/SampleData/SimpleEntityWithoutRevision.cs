using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Tests.SampleData
{
	public class SimpleEntityWithoutRevision: IEntity
	{
		public const string StandardRevision = "1-1a517022a0c2d4814d51abfedf9bfee7";
		public const string StandardEntityId = "doc1";
		public const string StandardDocId = "simpleEntityWithoutRevision.doc1";

		public static JObject OkResponse = new
		{
			ok = true,
			id = StandardDocId,
			rev = StandardRevision
		}.ToJObject();

		public static JObject DocumentWithRevision = new
		{
			_id = StandardDocId,
			_rev = StandardRevision,
			type = "simpleEntityWithoutRevision",
			name = "John Smith",
			age = 42,
			date = "1959-04-10T00:00:00"
		}.ToJObject();

		public static SimpleEntityWithoutRevision CreateStd()
		{
			return new SimpleEntityWithoutRevision
			  {
			    Id = StandardEntityId,
			    Name = "John Smith",
			    Age = 42,
			    Date = new DateTime(1957, 4, 10)
			  };
		}

		[JsonIgnore]
		public string Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public DateTime Date { get; set; }
	}
}