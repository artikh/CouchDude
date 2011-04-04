using System;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class SaveUpdateLoadAndDelete
	{
		[Fact]
		public void ShouldSaveUpdateAndLoadSimpleEntity()
		{
			var settings = new Settings(new Uri("http://127.0.0.1:5984"), "temp");
			var sessionFactory = new CouchSessionFactory(settings);

			var savedEntity = new SimpleEntity {
				Id = Guid.NewGuid().ToString(),
				Name = "John Smith",
				Age = 42,
				Date = DateTime.Now
			};
			SimpleEntity updatingEntity;
			SimpleEntity loadedEntity;

			using (var session = sessionFactory.CreateSession())
			{
				session.Save(savedEntity);
			}

			using (var session = sessionFactory.CreateSession())
			{
				updatingEntity = session.Load<SimpleEntity>(savedEntity.Id);
				updatingEntity.Name = "Artem Tikhomirov";
			}

			using (var session = sessionFactory.CreateSession())
			{
				loadedEntity = session.Load<SimpleEntity>(updatingEntity.Id);
				Assert.Equal("Artem Tikhomirov", loadedEntity.Name);
			}
			
			using (var session = sessionFactory.CreateSession())
				session.Delete(loadedEntity);
		}
	}
}
