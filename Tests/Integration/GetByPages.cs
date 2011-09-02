using System;
using System.Linq;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class GetByPages
	{
		[Fact]
		public void ShouldFetchDataByPagesUsingViewQueryResultNextPageQuery()
		{
			var prefix = Guid.NewGuid().ToString();

			var sessionFactory = new CouchSessionFactory(Default.Settings);

			using (var session = sessionFactory.CreateSession())
			{
				for (var i = 1; i <= 30; i++)
				{
					var id = string.Format("{0}_{1:00}", prefix, i);
					session.Save(new Entity { Id = id, Name = i.ToString()});
				}
				session.SaveChanges();
			}

			string nextPageQuery;

			using (var session = sessionFactory.CreateSession())
			{
				var result = session.Synchronously.Query<Entity>(new ViewQuery {
					ViewName = "_all_docs",
					StartKey = "entity." + prefix + "_00",
					EndKey = "entity." + prefix + "_30",
					Limit = 10,
					IncludeDocs = true
				});

				Assert.Equal(10, result.Count);

				var fourthResult = result.Skip(3).First();
				Assert.Equal(prefix + "_04", fourthResult.Id);
				Assert.Equal("4", fourthResult.Name);

				nextPageQuery = result.NextPageQuery.ToString();
			}
			
			using (var session = sessionFactory.CreateSession())
			{
				var result = session.Synchronously.Query<Entity>(ViewQuery.Parse(nextPageQuery));

				Assert.Equal(10, result.Count);

				var fifthResult = result.Skip(4).First();
				Assert.Equal(prefix + "_15", fifthResult.Id);
				Assert.Equal("15", fifthResult.Name);
			}
		}
	}
}