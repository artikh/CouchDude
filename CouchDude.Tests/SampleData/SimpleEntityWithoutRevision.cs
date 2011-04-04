using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchDude.Tests.SampleData
{
	public class SimpleEntityWithoutRevision
	{
		public const string StandardRevision = "1-1a517022a0c2d4814d51abfedf9bfee7";
		public const string StandardId = "doc1";

		public static JObject OkResponse = new
		{
			ok = true,
			id = StandardId,
			rev = StandardRevision
		}.ToJObject();

		public static JObject DocumentWithRevision = new
		{
			_id = StandardId,
			_rev = StandardRevision,
			type = "simpleEntityWithoutRevision",
			name = "John Smith",
			age = 42,
			date = "1959-04-10T00:00:00"
		}.ToJObject();

		public static SimpleEntityWithoutRevision Standard = new SimpleEntityWithoutRevision
		{
			Id = StandardId,
			Name = "John Smith",
			Age = 42,
			Date = new DateTime(1957, 4, 10)
		};

		[JsonIgnore]
		public string Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public DateTime Date { get; set; }
	}
}