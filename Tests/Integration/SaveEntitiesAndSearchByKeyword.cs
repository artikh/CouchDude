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
using System.Net.Http;
using CouchDude.Api;
using CouchDude.Serialization;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class SaveEntitiesAndSearchByKeyword
	{
		public SaveEntitiesAndSearchByKeyword()
		{
			var databaseApi = ((ICouchApi) new CouchApi(new CouchApiSettings("http://127.0.0.1:5984"), null)).Db("testdb");

			var luceneDoc = new {
				_id = "_design/lucene",
				language = "javascript",
				fulltext = new {
					test = new {
						index = @"
							function (doc) {
								if (doc.age == 424242) {
									var ret = new Document();
									// indexing fields
									ret.add(doc.name, {""field"": ""default""});
								}
							return null;
						}"
					}
				}
			}.ToDocument();

			databaseApi.Synchronously.Create(throwIfExists: false);
			var existingLucineDesignDocRevision = databaseApi.Synchronously.RequestLastestDocumentRevision("_design/lucene");
			if (existingLucineDesignDocRevision != null)
			{
				luceneDoc.Revision = existingLucineDesignDocRevision;
				databaseApi.Synchronously.SaveDocument(luceneDoc);
			}
			else
			{
				databaseApi.Synchronously.SaveDocument(luceneDoc);
			}
		}

		[Fact(Skip = "Do not work for some reason :)")]
		public void ShouldSaveTwoEntitiesAndFindTheyByKeyword()
		{
			var sessionFactory = Default.Settings.CreateSessionFactory();

			var entityA = new Entity
			{
				Name = "John Smith stas",
				Age = 424242,
				Date = DateTime.Now
			};

			var entityB = new Entity
			{
				Name = "Stas Girkin stas",
				Age = 424242,
				Date = DateTime.Now
			};

			using (var session = sessionFactory.CreateSession())
			{
				session.RawApi.Synchronously.Create(throwIfExists: false);
				session.Save(entityA);
				session.Save(entityB);
				session.SaveChanges();
			}

			using (var session = sessionFactory.CreateSession())
			{
				var result = session.Synchronously.QueryLucene<Entity>(
					new LuceneQuery { DesignDocumentName = "lucene", IndexName = "test", Query = "stas", IncludeDocs = true });
				Assert.True(result.Count >= 2);

				var loadedEntityA = result.First(e => e.Id == entityA.Id);
				var loadedEntityB = result.First(e => e.Id == entityB.Id);

				Assert.NotNull(loadedEntityA);
				Assert.Equal(entityA.Name,     loadedEntityA.Name);
				Assert.Equal(entityA.Age,      loadedEntityA.Age);
				Assert.Equal(entityA.Date,     loadedEntityA.Date);
				Assert.Equal(entityA.Revision, loadedEntityA.Revision);

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