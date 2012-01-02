using System;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class GetNoneByQuery
	{
		public class ViewData
		{
			public string Name { get; set; }
		}

		[Fact]
		public void ShouldReturnZeroEntitiesFromView()
		{
			var sessionFactory = Default.Settings.CreateSessionFactory();
			var id = Guid.NewGuid().ToString();

			using (var session = sessionFactory.CreateSession())
			{
				session.RawApi.Synchronously.Create(throwIfExists: false);

				var queryTask = session.Query<Entity>(
					new ViewQuery { ViewName = "_all_docs", Key = id, IncludeDocs = true });
				
				Assert.True(queryTask.Wait(5000), "View task wait timed out.");

				Assert.Equal(0, queryTask.Result.Count);
			}
		}

		[Fact]
		public void ShouldReturnZeroViewDataItemsFromView()
		{
			var sessionFactory = Default.Settings.CreateSessionFactory();
			var id = Guid.NewGuid().ToString();

			using (var session = sessionFactory.CreateSession())
			{
				session.RawApi.Synchronously.Create(throwIfExists: false);

				var queryTask = session.Query<ViewData>(
					new ViewQuery { ViewName = "_all_docs", Key = id, IncludeDocs = true });
				
				Assert.True(queryTask.Wait(5000), "View task wait timed out.");

				Assert.Equal(0, queryTask.Result.Count);
			}
		}
	}
}