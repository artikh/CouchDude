using System.Linq;
using CouchDude.Core;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Unit
{
	public class ViewQueryOfTTests
	{
		public class User
		{
			public string Name { get; set; }
			public int Age { get; set; }
		}

		[Fact]
		public void ShouldMapValueIfNoEntityConfigReturned()
		{
			IEntityConfigRepository entityConfigRepository = Default.Settings;

			var query = new ViewQuery<User>();
			var processedRows = query.ProcessRows(
				entityConfigRepository,
				new[]{
					new ViewResultRow("key1".ToJsonFragment(), new { age = 42, name = "John" }.ToJsonFragment(), null, null), 
					new ViewResultRow("key1".ToJsonFragment(), new { age = 24, name = "Jack" }.ToJsonFragment(), null, null),
				}).ToArray();

			Assert.Equal(2, processedRows.Length);
			Assert.Equal("John", processedRows[0].Name);
			Assert.Equal(42, processedRows[0].Age);
			Assert.Equal("Jack", processedRows[1].Name);
			Assert.Equal(24, processedRows[1].Age);
		}

		[Fact]
		public void ShouldMapDocumentEntityConfigReturned()
		{
			IEntityConfigRepository entityConfigRepository = Default.Settings;

			var query = new ViewQuery<SimpleEntity>();
			var processedRows = query.ProcessRows(
				entityConfigRepository,
				new[]{
					new ViewResultRow(SimpleEntity.StandardDocId.ToJsonFragment(), null, SimpleEntity.StandardDocId, SimpleEntity.DocWithRevision), 
					new ViewResultRow(SimpleEntity.StandardDocId.ToJsonFragment(), null, SimpleEntity.StandardDocId, SimpleEntity.DocWithRevision)
				}).ToArray();

			var sampleEntity = SimpleEntity.CreateStd();

			Assert.Equal(2, processedRows.Length);
			Assert.Equal(SimpleEntity.StandardEntityId, processedRows[0].Id);
			Assert.Equal(sampleEntity.Name, processedRows[0].Name);
			Assert.Equal(sampleEntity.Age, processedRows[0].Age);
			Assert.Equal(SimpleEntity.StandardEntityId, processedRows[1].Id);
			Assert.Equal(sampleEntity.Name, processedRows[1].Name);
			Assert.Equal(sampleEntity.Age, processedRows[1].Age);
		}
	}
}