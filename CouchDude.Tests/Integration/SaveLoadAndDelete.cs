using System;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class SaveLoadAndDelete
	{
		[Fact]
		public void ShouldSaveAndThanLoadSimpleEntity()
		{
			var settings = new Settings(new Uri("http://127.0.0.1:5984"), "temp");
			var sessionFactory = new CouchSessionFactory(settings);

			var savedEntity = new SimpleEntity {
				Id = Guid.NewGuid().ToString(),
				Name = "John Smith",
				Age = 42,
				Date = DateTime.Now
			};

			using (var session = sessionFactory.CreateSession())
			{
				var docInfo = session.Save(savedEntity);
				Assert.Equal(savedEntity.Id, docInfo.Id);
				Assert.NotNull(docInfo.Revision);
				Assert.NotNull(savedEntity.Revision);
			}

			using (var session = sessionFactory.CreateSession())
			{
				var loadedEntity = session.Load<SimpleEntity>(savedEntity.Id);

				Assert.Equal(savedEntity.Id, loadedEntity.Id);
				Assert.Equal(savedEntity.Revision, loadedEntity.Revision);
				Assert.Equal(savedEntity.Name, loadedEntity.Name);
				Assert.Equal(savedEntity.Age, loadedEntity.Age);
				Assert.Equal(savedEntity.Date, loadedEntity.Date);
			}

			using (var session = sessionFactory.CreateSession())
				session.Delete(savedEntity);
		}

		[Fact]
		public void ShouldSaveAndThanLoadSimpleEntityWithoutRevision()
		{
			var settings = new Settings(new Uri("http://127.0.0.1:5984"), "temp");
			var sessionFactory = new CouchSessionFactory(settings);

			var savedEntity = new SimpleEntityWithoutRevision {
				Id = Guid.NewGuid().ToString(),
				Name = "John Smith",
				Age = 42,
				Date = DateTime.Now
			};

			using (var session = sessionFactory.CreateSession())
			{
				var docInfo = session.Save(savedEntity);
				Assert.Equal(savedEntity.Id, docInfo.Id);
				Assert.NotNull(docInfo.Revision);
			}

			using (var session = sessionFactory.CreateSession())
			{
				var loadedEntity = session.Load<SimpleEntityWithoutRevision>(savedEntity.Id);

				Assert.Equal(savedEntity.Id, loadedEntity.Id);
				Assert.Equal(savedEntity.Name, loadedEntity.Name);
				Assert.Equal(savedEntity.Age, loadedEntity.Age);
				Assert.Equal(savedEntity.Date, loadedEntity.Date);

				var docInfo = session.Delete(loadedEntity);
				Assert.Equal(loadedEntity.Id, docInfo.Id);
				Assert.NotNull(docInfo.Revision);
			}
		}
	}
}
