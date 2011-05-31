using System;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Http;
using CouchDude.Core.Impl;
using CouchDude.Core.Utils;
using CouchDude.Tests.SampleData;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class SaveEntitiesAndSearchByKeyword
	{
		public SaveEntitiesAndSearchByKeyword()
		{
			var settings = Default.Settings;
			var couchApi = new CouchApi(new HttpClientImpl(), settings.ServerUri, settings.DatabaseName);

			var luceneDoc = new
			{
				_id = "_design/lucene",
				fulltext = new
				{
					index = @"
						function (doc) {
							if (doc.type == 'user') {
								var ret = new Document();
								ret.add(doc.username);
								return ret;
							}
						return null;
					}"
				}
			}.ToJObject();

			var existingLucineDesignDoc = couchApi.GetDocumentFromDbById("_design/lucene");
			string revision = existingLucineDesignDoc.GetRequiredProperty("_rev");
			if (existingLucineDesignDoc != null)
			{
				luceneDoc["_rev"] = JToken.FromObject(revision);
				couchApi.UpdateDocumentInDb("_design/lucene", luceneDoc);
			}
			else
			{
				couchApi.SaveDocumentToDb("_design/lucene", luceneDoc);
			}
		}

		[Fact(Skip = "No lucine")]
		public void ShouldSaveTwoEntitiesAndFindTheyByKeyword()
		{
			var sessionFactory = new CouchSessionFactory(Default.Settings);

			var entityA = new SimpleEntity
			{
				Name = "John Smith stas",
				Age = 42,
				Date = DateTime.Now
			};

			var entityB = new SimpleEntity
			{
				Name = "Stas Girkin stas",
				Age = 24,
				Date = DateTime.Now
			};

			using (var session = sessionFactory.CreateSession())
			{
				session.Save(entityA);
				session.Save(entityB);
			}

			using (var session = sessionFactory.CreateSession())
			{
				var result = session.FulltextQuery(new LuceneQuery<SimpleEntity> { DesignDocumentName = "lucene", IndexName = "all", Query = "stas", IncludeDocs = true });
				Assert.True(result.RowCount >= 2);

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
			}
		}
	}
}