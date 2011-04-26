using System;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class SaveAndGetAll
	{
		[Fact]
		public void ShouldSaveCoupleEntitiesAndThenGetThemAllDeletingAfterwards()
		{
			var sessionFactory = new CouchSessionFactory(Default.Settings);

			var entityA = new SimpleEntity
			{
				Name = "John Smith",
				Age = 42,
				Date = DateTime.Now
			};

			var entityB = new SimpleEntity
			{
				Name = "Stas Girkin",
				Age = 42,
				Date = DateTime.Now
			};

			using (var session = sessionFactory.CreateSession())
			{
				session.Save(entityA);
				session.Save(entityB);
			}

			using (var session = sessionFactory.CreateSession())
			{
				var result = session.Query(new ViewQuery<SimpleEntity> { ViewName = "_all_docs" });
				Assert.True(result.RowCount > 2);

				var loadedEntityA = result.Rows.First(e => e.Id == entityA.Id);
				var loadedEntityB = result.Rows.First(e => e.Id == entityB.Id);

				Assert.NotNull(loadedEntityA);
				Assert.Equal(entityA.Name,			loadedEntityA.Name);
				Assert.Equal(entityA.Age,				loadedEntityA.Age);
				Assert.Equal(entityA.Date,			loadedEntityA.Date);
				Assert.Equal(entityA.Revision,	loadedEntityA.Revision);

				Assert.NotNull(loadedEntityB);
				Assert.Equal(entityB.Name,			loadedEntityB.Name);
				Assert.Equal(entityB.Age,				loadedEntityB.Age);
				Assert.Equal(entityB.Date,			loadedEntityB.Date);
				Assert.Equal(entityB.Revision,	loadedEntityB.Revision);
			}

			using (var session = sessionFactory.CreateSession())
			{
				session.Delete(entityA);
				session.Delete(entityB);
			}
		}
	}
}