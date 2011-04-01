using System;
using Newtonsoft.Json;

namespace CouchDude.Tests.SampleData
{
	public class SimpleEntityWithoutRevision
	{
		[JsonIgnore]
		public string Id { get; set; }
		public string Name { get; set; }
		public int Age { get; set; }
		public DateTime Date { get; set; }
	}
}