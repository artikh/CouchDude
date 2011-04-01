using System;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchSessionSaveTests
	{
		public class TestEntity
		{
			public string Id { get; set; }

			public string Revision { get; set; }

			public string Name { get; set; }
		}
		
		private readonly Settings settings = 
			new Settings(new Uri("http://example.com"), "temp");

		private readonly TestEntity entity = new TestEntity {
			Id = "doc1",
			Name = "John Smith"
		};

		[Fact]
		public void ShouldThrowOnSameInstanseSave()
		{
			Assert.Throws<ArgumentException>(() => DoSave(
				action: session => {
					session.Save(entity);
					session.Save(entity);
					return new DocumentInfo();
				}));
		}

		[Fact]
		public void ShouldThrowOnSaveWithRevision()
		{
			Assert.Throws<ArgumentException>(() => 
				DoSave(
					new TestEntity {
						Id = "doc1",
						Revision = "42-1a517022a0c2d4814d51abfedf9bfee7",
						Name = "John Smith"
			}));
		}

		[Fact]
		public void ShouldThrowOnNullEntity()
		{
			var session = new CouchSession(settings, Mock.Of<ICouchApi>());
			Assert.Throws<ArgumentNullException>(() => session.Save<TestEntity>(null));
		}

		[Fact]
		public void ShouldReturnRevisionAndId()
		{
			var documentInfo = DoSave();
			Assert.Equal(entity.Id, documentInfo.Id);
			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", documentInfo.Revision);
		}

		[Fact]
		public void ShouldReturnFillRevisionPropertyOnEntity()
		{
			DoSave();
			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", entity.Revision);
		}

		private DocumentInfo DoSave(
			TestEntity savingEntity = null, 
			Mock<ICouchApi> couchApiMock = null,
			Func<ISession, DocumentInfo> action = null)
		{
			savingEntity = savingEntity ?? entity;
			if (couchApiMock == null)
			{
				couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
				couchApiMock
					.Setup(ca => ca.SaveDocumentToDb(It.IsAny<string>(), It.IsAny<JObject>()))
					.Returns(new {
						id = savingEntity == null? null: savingEntity.Id,
						rev = "42-1a517022a0c2d4814d51abfedf9bfee7"
					}.ToJObject());
			}
			
			var session = new CouchSession(settings, couchApiMock.Object);

			if (action == null)
				return session.Save(savingEntity);
			else
				return action(session);
		}
	}
}