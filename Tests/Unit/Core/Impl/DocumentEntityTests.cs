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

using System.IO;
using CouchDude.Api;
using CouchDude.Configuration;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
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

		private Entity entity = Entity.CreateStandardWithoutRevision();

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

			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", documentEntity.Document.Revision);
		}

		[Fact]
		public void ShouldReturnDocumentRevisionIfThereIsNoRevisionPropertyOnDocument()
		{
			var documentEntity = DocumentEntity.FromDocument(
				EntityWithoutRevision.CreateDocumentWithRevision(), Default.Settings);
			documentEntity.DoMap();

			Assert.Equal(EntityWithoutRevision.StandardRevision, documentEntity.Revision);
		}

		[Fact]
		public void ShouldReturnDocumentRevisionIfThereIsNoEntityCreatedYet()
		{
			var documentEntity = DocumentEntity.FromDocument(
				EntityWithoutRevision.CreateDocumentWithRevision(), Default.Settings);

			Assert.Equal(EntityWithoutRevision.StandardRevision, documentEntity.Revision);
		}

		[Fact]
		public void ShouldLoadAllDataFromEntity()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);

			Assert.Equal("doc1", documentEntity.EntityId);
			Assert.Null(documentEntity.Revision);
			Assert.Equal(typeof(Entity), documentEntity.EntityType);
			Assert.Equal("entity", documentEntity.DocumentType);
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
				new { _id = "entity.doc1", type = "entity", name = "John Smith", age = 42, date = "1957-04-10T00:00:00Z" }.ToJsonString(), 
				writtenString,
				new JTokenStringCompairer());
		}

		[Fact]
		public void ShouldDetectDifferenceIfJsonDocumentIsNull()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);
			entity.Name = "Joe Fox";

			Assert.Null(documentEntity.Document);
			Assert.True(documentEntity.MapIfChanged());
		}

		[Fact]
		public void ShouldDetectDifferenceAfterMap()
		{
			var documentEntity = DocumentEntity.FromEntity(entity, Default.Settings);
			documentEntity.DoMap();
			entity.Name = "Joe Fox";

			Assert.NotNull(documentEntity.Document);
			Assert.True(documentEntity.MapIfChanged());
		}

		[Fact]
		public void ShouldAutodeserializeEntityWhenCreatingFromJson()
		{
			var documentEntity = DocumentEntity.FromDocument(
				Entity.CreateDocWithRevision(), Default.Settings);

			Assert.NotNull(documentEntity);
			Assert.NotNull(documentEntity.Entity);
			Assert.Equal(typeof(Entity), documentEntity.EntityType);

			entity = (Entity)documentEntity.Entity;
			Assert.Equal(Entity.StandardEntityId, entity.Id);
			Assert.Equal(Entity.StandardRevision, entity.Revision);
			Assert.Equal("John Smith", entity.Name);
			Assert.Equal(42, entity.Age);
		}

		[Fact]
		public void ShouldSetDocumentWhenCreatingFromJson()
		{
			var documentEntity = DocumentEntity.FromDocument(
				Entity.CreateDocWithRevision(), Default.Settings);

			Assert.NotNull(documentEntity);
			Assert.NotNull(documentEntity.Document);
			Assert.Equal(Entity.CreateDocWithRevision(), documentEntity.Document);
		}
		
		[Fact]
		public void ShouldThrowCouchResponseParseExceptionOnDocumentWithoutType()
		{
			Assert.Throws<DocumentTypeMissingException>(
				() => DocumentEntity.FromDocument(
					new { _id = "entity.doc1", _rev = "42-1a517022a0c2d4814d51abfedf9bfee7", name = "John Smith" }.ToDocument(), 
					Default.Settings
			));
		}
	}
}