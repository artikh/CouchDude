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
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class SaveUpdateLoadAndDelete
	{
		[Fact]
		public void ShouldSaveUpdateAndLoadSimpleEntity()
		{
			var sessionFactory = Default.Settings.CreateSessionFactory();

			var savedEntity = new Entity {
				Id = Guid.NewGuid().ToString(),
				Name = "John Smith",
				Age = 42,
				Date = DateTime.Now
			};
			Entity updatingEntity;
			Entity loadedEntity;

			using (var session = sessionFactory.CreateSession())
			{
				session.Save(savedEntity);
				session.SaveChanges();
			}

			using (var session = sessionFactory.CreateSession())
			{
				updatingEntity = session.Synchronously.Load<Entity>(savedEntity.Id);
				updatingEntity.Name = "Artem Tikhomirov";
				session.SaveChanges();
			}

			using (var session = sessionFactory.CreateSession())
			{
				loadedEntity = session.Synchronously.Load<Entity>(updatingEntity.Id);
				Assert.Equal("Artem Tikhomirov", loadedEntity.Name);
			}
			
			using (var session = sessionFactory.CreateSession())
			{
				session.Delete(loadedEntity);
				session.SaveChanges();
			}

		}
	}
}
