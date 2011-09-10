using System;
using System.Linq;
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	public class Animal
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class Cat: Animal { }

	public class ViciousCat: Cat { }

	public class Chinchilla: Animal { }

	[IntegrationTest]
	public class InheritanceSupport
	{
		[Fact]
		public void ShouldSaveAndLoadDerivedEntities() 
		{
			var sessionFactory = ConfigureCouchDude.With()
					.ServerUri("http://127.0.0.1:5984")
					.DefaultDatabaseName("testdb")
					.MappingEntities()
						.FromAssemblyOf<Animal>()
						.InheritedFrom<Animal>()
						// It's crutual for now to ensure whole inheritance heirarchy have same id convertion rules
						.TranslatingEntityIdToDocumentIdAs((id, type, documentType) => id)
						.TranslatingDocumentIdToEntityIdAs((id, type, entityType) => id)
					.CreateSettings()
					.CreateSessionFactory();

			var musa = new Cat { Name = "Musa" };
			var fluffy = new ViciousCat { Name = "Fluffy" };
			var zoidberg = new Chinchilla { Name = "Dr. Zoidberg" };

			using(var session = sessionFactory.CreateSession())
			{
				session.Save<Animal>(musa, fluffy, zoidberg);
				session.SaveChanges();
			}

			using (var session = sessionFactory.CreateSession())
			{
				var loadedMusa = session.Synchronously.Load<Animal>(musa.Id);
				Assert.IsType<Cat>(loadedMusa);

				var loadedFluffy = session.Synchronously.Load<Animal>(fluffy.Id);
				Assert.IsType<ViciousCat>(loadedFluffy);

				var loadedZoidberg = session.Synchronously.Load<Animal>(zoidberg.Id);
				Assert.IsType<Chinchilla>(loadedZoidberg);
			}
		}

		[Fact]
		public void ShouldSaveAndQueryDerivedEntities() 
		{
			var sessionFactory = ConfigureCouchDude.With()
					.ServerUri("http://127.0.0.1:5984")
					.DefaultDatabaseName("testdb")
					.MappingEntities()
						.FromAssemblyOf<Animal>()
						.InheritedFrom<Animal>()
						// It's crutual for now to ensure whole inheritance heirarchy have same id convertion rules
						.TranslatingEntityIdToDocumentIdAs((id, type, documentType) => id)
						.TranslatingDocumentIdToEntityIdAs((id, type, entityType) => id)
					.CreateSettings()
					.CreateSessionFactory();

			var prefix = Guid.NewGuid() + ".";
			var musa = new Cat { Id = prefix + "0", Name = "Musa" };
			var fluffy = new ViciousCat { Id = prefix + "1", Name = "Fluffy" };
			var zoidberg = new Chinchilla { Id = prefix + "2", Name = "Dr. Zoidberg" };

			using(var session = sessionFactory.CreateSession())
			{
				session.Save<Animal>(musa, fluffy, zoidberg);
				session.SaveChanges();
			}

			using (var session = sessionFactory.CreateSession())
			{
				var animals = session.Synchronously
					.Query<Animal>(new ViewQuery { ViewName = "_all_docs", StartKey = musa.Id, EndKey = zoidberg.Id, IncludeDocs = true })
					.ToDictionary(a => a.Id, a => a);

				Assert.IsType<Cat>(animals[musa.Id]);
				Assert.IsType<ViciousCat>(animals[fluffy.Id]);
				Assert.IsType<Chinchilla>(animals[zoidberg.Id]);
			}
		}
	}
}
