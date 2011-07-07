using System;
using System.Diagnostics;
using System.Linq;
using CouchDude.Core;
using CouchDude.Core.Configuration;
using CouchDude.Tests.SampleData;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Configuration
{
	public class DefaultEntityConfigConventionsTests
	{	
		// ReSharper disable InconsistentNaming
		// ReSharper disable UnusedMember.Local
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		public class RevPrivatePropertyEntity { private string Rev { get; set; } }
		public class RevisionPrivatePropertyEntity { private string Revision { get; set; } }
		public class RevisionPublicPropertyEntity { public string Revision { get; set; } }
		public class RevPublicPropertyEntity { public string Rev { get; set; } }
		public class RevPublicFieldEntity { public string Rev; }
		public class RevisionPublicFieldEntity { public string Revision; }

		public class IDPublicPropertyPlusIdPrivatePropertyEntity { 
			public string Id { get; set; }
			private string ID { get; set; }
		}
		public class IdPrivatePropertyEntity { private string Id { get; set; } }
		public class IDPrivatePropertyEntity { private string ID { get; set; } }
		public class IdPublicPropertyEntity { public string Id { get; set; } }
		public class IDPublicPropertyEntity { public string ID { get; set; } }
		public class IdPublicFieldEntity { public string Id; }	
		public class IDPublicFieldEntity { public string ID; }

		public class NormalIdAndRevEntity
		{
			public string Id { get; private set; }
			public string Revision { get; private set; }  
		}
		// ReSharper restore UnusedMember.Local
		// ReSharper restore InconsistentNaming	
		// ReSharper restore UnusedAutoPropertyAccessor.Local
		
		[Theory]
		[InlineData(typeof(RevPrivatePropertyEntity))]
		[InlineData(typeof(RevisionPrivatePropertyEntity))]
		[InlineData(typeof(RevisionPublicPropertyEntity))]
		[InlineData(typeof(RevPublicPropertyEntity))]
		[InlineData(typeof(RevPublicFieldEntity))]
		[InlineData(typeof(RevisionPublicFieldEntity))]
		[InlineData(typeof(NormalIdAndRevEntity))]
		public void ShouldSetAndGetRevision(Type entityType)
		{
			var revision = Guid.NewGuid().ToString();
			var entity = Activator.CreateInstance(entityType);
			DefaultEntityConfigConventions.SetEntityRevisionIfPosssible(revision, entity, entityType);
			var returnedRevision = DefaultEntityConfigConventions.GetEntityRevisionIfPossible(entity, entityType);

			Assert.Equal(revision, returnedRevision);
		}
		
		[Theory]
		[InlineData(typeof(IdPrivatePropertyEntity))]
		[InlineData(typeof(IDPrivatePropertyEntity))]
		[InlineData(typeof(IdPublicPropertyEntity))]
		[InlineData(typeof(IDPublicPropertyEntity))]
		[InlineData(typeof(IdPublicFieldEntity))]
		[InlineData(typeof(IDPublicFieldEntity))]
		[InlineData(typeof(NormalIdAndRevEntity))]
		[InlineData(typeof(IDPublicPropertyPlusIdPrivatePropertyEntity))]
		public void ShouldSetAndGetId(Type entityType)
		{
			var revision = Guid.NewGuid().ToString();
			var entity = Activator.CreateInstance(entityType);
			var result = DefaultEntityConfigConventions.TrySetEntityId(revision, entity, entityType);
			Assert.True(result);
			
			string returnedRevision;
			result = DefaultEntityConfigConventions.TryGetEntityId(entity, entityType, out returnedRevision);
			Assert.True(result);

			Assert.Equal(revision, returnedRevision);
		}

		[Fact]
		public void ShouldReturnFalseIfNoIdProperty()
		{
			var setResult = DefaultEntityConfigConventions.TrySetEntityId("rev1", new RevisionPublicFieldEntity(), typeof(RevisionPublicFieldEntity));
			Assert.False(setResult);

			string entityType;
			var getResult = DefaultEntityConfigConventions.TryGetEntityId(
				new RevisionPublicFieldEntity(), typeof (RevisionPublicFieldEntity), out entityType);
			Assert.False(getResult);
		}

		[Fact]
		public void ShouldConvertEntityTypeShortNameToCamelCaseWhenTransformingItToDocumnetType()
		{
			Assert.Equal("simpleEntity", DefaultEntityConfigConventions.EntityTypeToDocumentType(typeof(SimpleEntity)));
		}

		[Theory]
		[InlineData(typeof(IdPrivatePropertyEntity)				             , new[]{ "Id" })]
		[InlineData(typeof(IDPrivatePropertyEntity)				             , new[]{ "ID" })]
		[InlineData(typeof(IdPublicPropertyEntity)				             , new[]{ "Id" })]
		[InlineData(typeof(IDPublicPropertyEntity)				             , new[]{ "ID" })]
		[InlineData(typeof(IdPublicFieldEntity)						             , new[]{ "Id" })]
		[InlineData(typeof(IDPublicFieldEntity)						             , new[]{ "ID" })]
		[InlineData(typeof(RevPrivatePropertyEntity)			             , new[]{ "Rev" })]
		[InlineData(typeof(RevisionPrivatePropertyEntity)	             , new[]{ "Revision" })]
		[InlineData(typeof(RevisionPublicPropertyEntity)	             , new[]{ "Revision" })]
		[InlineData(typeof(RevPublicPropertyEntity)				             , new[]{ "Rev" })]
		[InlineData(typeof(RevPublicFieldEntity)					             , new[]{ "Rev" })]
		[InlineData(typeof(RevisionPublicFieldEntity)                  , new[]{ "Revision" })]
		[InlineData(typeof(NormalIdAndRevEntity)                       , new[]{ "Id", "Revision" })]
		[InlineData(typeof(IDPublicPropertyPlusIdPrivatePropertyEntity), new[]{ "Id" })]
		public void ShouldReturnFoundIDAndRevisionMembersAsIgnored(Type entityType, string[] expectedIgnoredMemberNames)
		{
			var ignoredMembers = DefaultEntityConfigConventions.GetIgnoredMembers(entityType).ToList();
			Assert.Equal(expectedIgnoredMemberNames.Length, ignoredMembers.Count);
			
			foreach (var ignoredMember in ignoredMembers)
				Assert.Contains(ignoredMember.Name, expectedIgnoredMemberNames);
		}
	}
}
