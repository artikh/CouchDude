#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
	Licensed under the Apache License, Version 2.0 (the "License");					
	you may not use this file except in compliance with the License.					
	You may obtain a copy of the License at																	
																																					
	    http://www.apache.org/licenses/LICENSE-2.0														
																																					
	Unless required by applicable law or agreed to in writing, software			
	distributed under the License is distributed on an "AS IS" BASIS,				
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.	
	See the License for the specific language governing permissions and			
	limitations under the License.																						
*/
#endregion

using System;
using System.Linq;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class SaveAndGetAll
	{
		[Fact]
		public void ShouldSaveCoupleEntitiesAndThenGetThemAllDeletingAfterwards()
		{
			var sessionFactory = Default.Settings.CreateSessionFactory();
			var prefix = Guid.NewGuid().ToString();

			var entityA = new Entity
			{
				Id = prefix + "1",
				Name = "1",
				Age = 42,
				Date = DateTime.UtcNow
			};

			var entityB = new Entity
			{
				Id = prefix + "2",
				Name = "Stas Girkin",
				Age = 42,
				Date = DateTime.UtcNow
			};

			using (var session = sessionFactory.CreateSession())
			{
				session.Save(entityA);
				session.Save(entityB);
				session.SaveChanges();
			}

			using (var session = sessionFactory.CreateSession())
			{
				var result = session.Synchronously.Query<Entity>(new ViewQuery {
					ViewName = "_all_docs",
					StartKey = "entity." + entityA.Id,
					EndKey = "entity." + entityB.Id,
					IncludeDocs = true
				});
				Assert.True(result.Count >= 2);

				var loadedEntityA = result.First(e => e != null && e.Id == entityA.Id);
				var loadedEntityB = result.First(e => e != null && e.Id == entityB.Id);

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
				session.SaveChanges();
			}
		}
	}
}