﻿#region Licence Info 
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
using System.Linq;
using CouchDude.Impl;
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
				var result = session.Synchronously.Query(new ViewQuery<SimpleEntity> { ViewName = "_all_docs", IncludeDocs = true });
				Assert.True(result.RowCount >= 2);

				var loadedEntityA = result.First(e => e.Id == entityA.Id);
				var loadedEntityB = result.First(e => e.Id == entityB.Id);

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