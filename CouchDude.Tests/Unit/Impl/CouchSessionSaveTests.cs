using System;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Impl
{
	public class CouchSessionSaveTests
	{
		private readonly SimpleEntity entity = new SimpleEntity {
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
					new SimpleEntity {
						Id = "doc1",
						Revision = "42-1a517022a0c2d4814d51abfedf9bfee7",
						Name = "John Smith"
			}));
		}

		[Fact]
		public void ShouldThrowOnNullEntity()
		{
			var session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			Assert.Throws<ArgumentNullException>(() => session.Save<SimpleEntity>(null));
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

		[Fact]
		public void ShouldAssignOnSaveIfNoneWasAssignedBefore()
		{
			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.SaveDocumentToDb(It.IsAny<string>(), It.IsAny<JObject>()))
				.Returns(
					(string docId, JObject doc) =>
					new
					{
						id = docId,
						rev = "1-1a517022a0c2d4814d51abfedf9bfee7"
					}.ToJObject()
				);

			var savingEntity = new SimpleEntity
			{
				Name = "John Smith"
			};
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			var docInfo = session.Save(savingEntity);

			Assert.NotNull(savingEntity.Id);
			Assert.NotEqual(string.Empty, savingEntity.Id);
			Assert.Equal(savingEntity.Id, docInfo.Id);
		}

		private DocumentInfo DoSave(
			SimpleEntity savingEntity = null, 
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
			
			var session = new CouchSession(Default.Settings, couchApiMock.Object);

			if (action == null)
				return session.Save(savingEntity);
			else
				return action(session);
		}
	}
}