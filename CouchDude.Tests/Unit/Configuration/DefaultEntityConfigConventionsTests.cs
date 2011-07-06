using System;
using System.Diagnostics;
using CouchDude.Core;
using CouchDude.Core.Configuration;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Configuration
{
	public class DefaultEntityConfigConventionsTests
	{	
		// ReSharper disable InconsistentNaming
		// ReSharper disable UnusedMember.Local
		public class RevPrivatePropertyEntity { private string Rev { get; set; } }
		public class RevisionPrivatePropertyEntity { private string Revision { get; set; } }
		public class RevisionPublicPropertyEntity { public string Revision { get; set; } }
		public class RevPublicPropertyEntity { public string Rev { get; set; } }
		public class RevPublicFieldEntity { public string Rev; }
		public class RevisionPublicFieldEntity { public string Revision; }

		public class IdPrivatePropertyEntity { private string Id { get; set; } }
		public class IDPrivatePropertyEntity { private string ID { get; set; } }
		public class IdPublicPropertyEntity { public string Id { get; set; } }
		public class IDPublicPropertyEntity { public string ID { get; set; } }
		public class IdPublicFieldEntity { public string Id; }	
		public class IDPublicFieldEntity { public string ID; }
		// ReSharper restore UnusedMember.Local
		// ReSharper restore InconsistentNaming
		
		[Theory]
		[InlineData(typeof(RevPrivatePropertyEntity))]
		[InlineData(typeof(RevisionPrivatePropertyEntity))]
		[InlineData(typeof(RevisionPublicPropertyEntity))]
		[InlineData(typeof(RevPublicPropertyEntity))]
		[InlineData(typeof(RevPublicFieldEntity))]
		[InlineData(typeof(RevisionPublicFieldEntity))]
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
	}
}
