#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
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
using CouchDude.Core;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class SaveLoadAndDelete
	{
		[Fact]
		public void ShouldSaveLoadAndThanGetAllSimpleEntities()
		{
			var sessionFactory = new CouchSessionFactory(Default.Settings);

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

				Assert.NotNull(loadedEntity);
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
			var sessionFactory = new CouchSessionFactory(Default.Settings);

			var savedEntity = new SimpleEntityWithoutRevision
			                  	{
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
