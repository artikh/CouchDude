using System;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration.Lucene
{
	public class SaveEntitiesAndSearchByKeyword
	{
		/*
		 * Must have design doc - _design/lucene and couchdb-lucene 
		 * 
		 * {
		 *	   "_id": "_design/lucene",
		 *	   "_rev": "7-593ab29fd6747fcb1eb0e7bc3586a770",
		 *	   "fulltext": {
		 *		   "all": {
		 *			   "index": "function (doc) {\u000d\u000a\u0009if (doc.type == 'user') {\u000d\u000a\u0009\u0009var ret = new Document();\u000d\u000a\u0009\u0009ret.add(doc.username);\u000d\u000a\u0009\u0009return ret;\u000d\u000a\u0009}\u000d\u000a\u0009return null;\u000d\u000a}"
		 *		   }
		 *	   }
		 *	}
		 */
		[Fact]
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