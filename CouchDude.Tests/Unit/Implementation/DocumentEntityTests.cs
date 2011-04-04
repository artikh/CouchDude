using System;
using System.IO;
using CouchDude.Core;
using CouchDude.Core.Implementation;
using Newtonsoft.Json;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class DocumentEntityTests
	{
		public class TestEntity
		{
			[JsonIgnore]
			public string Id { get; set; }

			[JsonIgnore]
			public string Revision { get; set; }

			public string Name { get; set; }

			public int Age { get; set; }
		}

		public class NoIdGetterEntity
		{
			[JsonIgnore]
			// ReSharper disable ValueParameterNotUsed
			public string Id { set { } }
			// ReSharper restore ValueParameterNotUsed
		}

		private TestEntity entity = new TestEntity {
			Id = "doc1",
			Name = "John Smith",
			Age = 42
		};

		private readonly Settings settings = new Settings(new Uri("http://example.com"), "testdb");

		[Fact]
		public void ShouldSetRevisionOnUnmappedDocumentEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, settings);
			documentEntity.Revision = "42-1a517022a0c2d4814d51abfedf9bfee7";

			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", entity.Revision);
		}

		[Fact]
		public void ShouldSetRevisionOnMappedDocumentEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, settings);
			documentEntity.DoMap();
			documentEntity.Revision = "42-1a517022a0c2d4814d51abfedf9bfee7";

			Assert.Equal(
				"42-1a517022a0c2d4814d51abfedf9bfee7", 
				documentEntity.Document.Value<string>("_rev"));
		}

		[Fact]
		public void ShouldLoadAllDataFromEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, settings);

			Assert.Equal("doc1", documentEntity.Id);
			Assert.Equal(null, documentEntity.Revision);
			Assert.Equal(typeof(TestEntity), documentEntity.EntityType);
			Assert.Equal("testEntity", documentEntity.DocumentType);
			Assert.Same(entity, documentEntity.Entity);
		}

		[Fact]
		public void ShouldThrowConventionExceptionIfNoIdGetterFoundWhenCreatingFromEntity()
		{
			Assert.Throws<ConventionException>(() =>
				DocumentEntity.FromEntity(new NoIdGetterEntity(), settings));
		}

		[Fact]
		public void ShouldWriteProperJsonToTextWiter()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, settings);

			string writtenString;
			using (var writer = new StringWriter())
			{
				documentEntity.WriteTo(writer);
				writer.Flush();
				writtenString = writer.GetStringBuilder().ToString();
			}

			Assert.Equal(
				new { _id = "doc1", type = "testEntity", name = "John Smith", age = 42 }.ToJson(), 
				writtenString,
				new JTokenStringCompairer());
		}

		[Fact]
		public void ShouldNotDetectDifferenceIfJsonDocumentIsNull()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, settings);
			entity.Name = "Joe Fox";

			Assert.Null(documentEntity.Document);
			Assert.False(documentEntity.CheckIfChanged());
		}

		[Fact]
		public void ShouldDetectDifferenceAfterMap()
		{
			
			var documentEntity = DocumentEntity.FromEntity(entity, settings);
			documentEntity.DoMap();
			entity.Name = "Joe Fox";

			Assert.NotNull(documentEntity.Document);
			Assert.True(documentEntity.CheckIfChanged());
		}

		[Fact]
		public void ShouldAutodeserializeEntityWhenCreatingFromJson()
		{
			var documentEntity = DocumentEntity.FromJson<TestEntity>(new { 
				_id = "doc1",
				_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
				type = "testEntity",
				name = "John Smith",
				age = 42
			}.ToJObject(),
			settings);

			Assert.NotNull(documentEntity);
			Assert.NotNull(documentEntity.Entity);
			Assert.Equal(typeof(TestEntity), documentEntity.EntityType);

			entity = (TestEntity)documentEntity.Entity;
			Assert.Equal("doc1", entity.Id);
			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", entity.Revision);
			Assert.Equal("John Smith", entity.Name);
			Assert.Equal(42, entity.Age);
		}

		[Fact]
		public void ShouldSetDocumentWhenCreatingFromJson()
		{
			var documentEntity = DocumentEntity.FromJson<TestEntity>(new { 
				_id = "doc1",
				_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
				type = "testEntity",
				name = "John Smith",
				age = 42
			}.ToJObject(),
			settings);

			Assert.NotNull(documentEntity);
			Assert.NotNull(documentEntity.Document);
			Utils.AssertSameJson(
				new {
					_id = "doc1",
					_rev = "42-1a517022a0c2d4814d51abfedf9bfee7",
					type = "testEntity",
					name = "John Smith",
					age = 42
				},
				documentEntity.Document);
		}

		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutId()
		{
			Assert.Throws<CouchResponseParseException>(
				() => DocumentEntity.FromJson<TestEntity>(
					new {
						_rev = "42-1a517022a0c2d4814d51abfedf9bfee7", 
						type = "testEntity", 
						name = "John Smith"
					}.ToJObject(), 
					settings
			));
		}

		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutRevision()
		{
			Assert.Throws<CouchResponseParseException>(
				() => DocumentEntity.FromJson<TestEntity>(
					new { _id = "doc1", type = "testEntity", name = "John Smith" }.ToJObject(), 
					settings
			));
		}

		[Fact]
		public void ShouldThrowDocumentParseExceptionOnDocumentWithoutType()
		{
			Assert.Throws<CouchResponseParseException>(
				() => DocumentEntity.FromJson<TestEntity>(
					new { _id = "doc1", _rev = "42-1a517022a0c2d4814d51abfedf9bfee7", name = "John Smith" }.ToJObject(), 
					settings
			));
		}

		[Fact]
		public void ShouldThrowEntityTypeMismatchExceptionOnWrongDocumentType()
		{
			var ex = Assert.Throws<EntityTypeMismatchException>(
				() => DocumentEntity.FromJson<TestEntity>(
					new { _id = "doc1", _rev = "42-1a517022a0c2d4814d51abfedf9bfee7", type = "anotherEntity", name = "John Smith" }.ToJObject(), 
					settings
			));

			Assert.Contains("TestEntity", ex.Message);
			Assert.Contains("anotherEntity", ex.Message);
		}

		[Fact]
		public void ShouldSetIdOnEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, settings);
			documentEntity.SetId("new_id");
			Assert.Equal("new_id", entity.Id);
		}

		[Fact]
		public void ShouldSetIdOnDocument()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, settings);
			documentEntity.DoMap();
			documentEntity.SetId("new_id");
			Assert.Equal("new_id", documentEntity.Document.Value<string>("_id"));
		}
	}
}