using System.IO;
using CouchDude.Core;
using CouchDude.Core.Configuration;
using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CouchDude.Tests.Unit.Impl
{
	public class DocumentEntityTests
	{
		public class NoIdGetterEntity: IEntity
		{
			[JsonIgnore]
			// ReSharper disable ValueParameterNotUsed
			public string Id { set { } }
			// ReSharper restore ValueParameterNotUsed
		}

		private SimpleEntity entity = SimpleEntity.CreateStdWithoutRevision();

		[Fact]
		public void ShouldSetRevisionOnUnmappedDocumentEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);
			documentEntity.Revision = "42-1a517022a0c2d4814d51abfedf9bfee7";

			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", entity.Revision);
		}

		[Fact]
		public void ShouldSetRevisionOnMappedDocumentEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);
			documentEntity.DoMap();
			documentEntity.Revision = "42-1a517022a0c2d4814d51abfedf9bfee7";

			Assert.Equal(
				"42-1a517022a0c2d4814d51abfedf9bfee7", 
				documentEntity.Document.Value<string>("_rev"));
		}

		[Fact]
		public void ShouldReturnDocumentRevisionIfThereIsNoRevisionPropertyOnDocument()
		{
			var documentEntity = DocumentEntity.FromJson<SimpleEntityWithoutRevision>(
				SimpleEntityWithoutRevision.DocumentWithRevision, Default.Settings);
			documentEntity.DoMap();

			Assert.Equal(SimpleEntityWithoutRevision.StandardRevision, documentEntity.Revision);
		}

		[Fact]
		public void ShouldReturnDocumentRevisionIfThereIsNoEntityCreatedYet()
		{
			var documentEntity = DocumentEntity.FromJson<SimpleEntityWithoutRevision>(
				SimpleEntityWithoutRevision.DocumentWithRevision, Default.Settings);

			Assert.Equal(SimpleEntityWithoutRevision.StandardRevision, documentEntity.Revision);
		}

		[Fact]
		public void ShouldLoadAllDataFromEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);

			Assert.Equal("doc1", documentEntity.EntityId);
			Assert.Null(documentEntity.Revision);
			Assert.Equal(typeof(SimpleEntity), documentEntity.EntityType);
			Assert.Equal("simpleEntity", documentEntity.DocumentType);
			Assert.Same(entity, documentEntity.Entity);
		}

		[Fact]
		public void ShouldWriteProperJsonToTextWiter()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);

			string writtenString;
			using (var writer = new StringWriter())
			{
				documentEntity.WriteTo(writer);
				writer.Flush();
				writtenString = writer.GetStringBuilder().ToString();
			}

			Assert.Equal(
				new { _id = "simpleEntity.doc1", type = "simpleEntity", name = "John Smith", age = 42, date = "1957-04-10T00:00:00" }.ToJson(), 
				writtenString,
				new JTokenStringCompairer());
		}

		[Fact]
		public void ShouldNotDetectDifferenceIfJsonDocumentIsNull()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);
			entity.Name = "Joe Fox";

			Assert.Null(documentEntity.Document);
			Assert.False(documentEntity.CheckIfChanged());
		}

		[Fact]
		public void ShouldDetectDifferenceAfterMap()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);
			documentEntity.DoMap();
			entity.Name = "Joe Fox";

			Assert.NotNull(documentEntity.Document);
			Assert.True(documentEntity.CheckIfChanged());
		}

		[Fact]
		public void ShouldAutodeserializeEntityWhenCreatingFromJson()
		{
			var documentEntity = DocumentEntity.FromJson<SimpleEntity>(new {
				_id = "simpleEntity.doc1",
				_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
				type = "simpleEntity",
				name = "John Smith",
				age = 42
			}.ToJObject(),
			Default.Settings);

			Assert.NotNull(documentEntity);
			Assert.NotNull(documentEntity.Entity);
			Assert.Equal(typeof(SimpleEntity), documentEntity.EntityType);

			entity = (SimpleEntity)documentEntity.Entity;
			Assert.Equal("doc1", entity.Id);
			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", entity.Revision);
			Assert.Equal("John Smith", entity.Name);
			Assert.Equal(42, entity.Age);
		}

		[Fact]
		public void ShouldSetDocumentWhenCreatingFromJson()
		{
			var documentEntity = DocumentEntity.FromJson<SimpleEntity>(new {
				_id = "simpleEntity.doc1",
				_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
				type = "simpleEntity",
				name = "John Smith",
				age = 42
			}.ToJObject(),
			Default.Settings);

			Assert.NotNull(documentEntity);
			Assert.NotNull(documentEntity.Document);
			TestUtils.AssertSameJson(
				new {
					_id = "simpleEntity.doc1",
					_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
					type = "simpleEntity",
					name = "John Smith",
					age = 42
				},
				documentEntity.Document);
		}
		
		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutRevision()
		{
			Assert.Throws<CouchResponseParseException>(
				() => DocumentEntity.FromJson<SimpleEntity>(
					new { _id = "simpleEntity.doc1", type = "simpleEntity", name = "John Smith" }.ToJObject(), 
					Default.Settings
			));
		}

		[Fact]
		public void ShouldThrowCouchResponseParseExceptionOnDocumentWithoutType()
		{
			Assert.Throws<CouchResponseParseException>(
				() => DocumentEntity.FromJson<SimpleEntity>(
					new { _id = "simpleEntity.doc1", _rev = "42-1a517022a0c2d4814d51abfedf9bfee7", name = "John Smith" }.ToJObject(), 
					Default.Settings
			));
		}

		[Fact]
		public void ShouldThrowEntityTypeMismatchExceptionOnWrongDocumentType()
		{
			var ex = Assert.Throws<EntityTypeMismatchException>(
				() => DocumentEntity.FromJson<SimpleEntity>(
					new { _id = "simpleEntity.doc1", _rev = "42-1a517022a0c2d4814d51abfedf9bfee7", type = "anotherEntity", name = "John Smith" }.ToJObject(), 
					Default.Settings
			));

			Assert.Contains("SimpleEntity", ex.Message);
			Assert.Contains("anotherEntity", ex.Message);
		}

		[Fact]
		public void ShouldSetIdIfNoneWasSetBefore()
		{
			var savingEntity = new SimpleEntity
			{
				Name = "John Smith",
				Age = 42
			};
			var settings1 = Default.Settings;
			settings1.IdGenerator = Mock.Of<IIdGenerator>(g => g.GenerateId() == "generated_id");

			var documentEntity = DocumentEntity.FromEntity(savingEntity, settings1);

			Assert.Equal("generated_id", documentEntity.EntityId);
			Assert.Equal("generated_id", savingEntity.Id);
		}
	}
}