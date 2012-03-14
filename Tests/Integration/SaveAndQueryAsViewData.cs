using System;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class SaveAndQueryAsViewData
	{
		public class ViewData
		{
			public string Name { get; set; }
		}

		[Fact]
		public void ShouldReturnZeroElementsFromView()
		{
			var sessionFactory = Default.Settings.CreateSessionFactory();

			var prefix = Guid.NewGuid().ToString();

			using (var session = sessionFactory.CreateSession())
			{
				session.RawApi.Synchronously.Create(throwIfExists: false);

				session.RawApi.Synchronously.SaveDocument(
					new {
						_id = "_design/" + prefix,
						views = new {
							query = new { 
								map = @"
									function(doc) {
										if(doc._id.indexOf('" + prefix + @"') == 0) {
											emit(doc._id.substring(" + (prefix.Length + 1) + @"), { name: doc.name});
										}
									}"
							}
						}
					}.ToDocument(),
					overwriteConcurrentUpdates: true
				);
			}

			var entityA = new Entity
			{
				Id = prefix + "1",
				Name = "Mikhail Anikeev-Naumenko",
			};

			var entityB = new Entity
			{
				Id = prefix + "2",
				Name = "Stas Girkin",
			};

			using (var session = sessionFactory.CreateSession())
			{
				session.Save(entityA);
				session.Save(entityB);
				session.SaveChanges();
			}

			using (var session = sessionFactory.CreateSession())
			{
				var queryTask = session.Query<ViewData>(
					new ViewQuery { DesignDocumentName = prefix, ViewName = "query" });
				
				Assert.True(queryTask.Wait(5000), "View task wait timed out.");

				Assert.Equal(0, queryTask.Result.Count);
			}
		}
	}
}