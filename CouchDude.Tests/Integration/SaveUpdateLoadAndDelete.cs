using System;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class SaveUpdateLoadAndDelete
	{
		[Fact]
		public void ShouldSaveUpdateAndLoadSimpleEntity()
		{
			var sessionFactory = new CouchSessionFactory(Default.Settings);

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
				session.Flush();
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
