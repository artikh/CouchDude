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

using System;
using CouchDude.Configuration;
using CouchDude.Tests.SampleData;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Configuration
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
			var descriptor = DefaultEntityConfigConventions.GetRevisionMember(entityType);
			descriptor.SetValue(entity, revision);
			var returnedRevision = descriptor.GetValue(entity);

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
			var id = Guid.NewGuid().ToString();
			var entity = Activator.CreateInstance(entityType);
			var descriptor = DefaultEntityConfigConventions.GetIdMember(entityType);
			descriptor.SetValue(entity, id);

			string returnedRevision = descriptor.GetValue(entity);
			Assert.Equal(id, returnedRevision);
		}

		[Fact]
		public void ShouldReturnFalseIfNoIdProperty()
		{
			var isPresent = DefaultEntityConfigConventions.GetIdMember(typeof(RevisionPublicFieldEntity)).IsDefined;
			Assert.False(isPresent);
		}

		[Fact]
		public void ShouldReturnFalseIfNoRevisionProperty()
		{
			var isPresent = DefaultEntityConfigConventions.GetRevisionMember(typeof(IdPrivatePropertyEntity)).IsDefined;
			Assert.False(isPresent);
		}

		[Fact]
		public void ShouldConvertEntityTypeShortNameToCamelCaseWhenTransformingItToDocumnetType()
		{
			Assert.Equal("entity", DefaultEntityConfigConventions.EntityTypeToDocumentType(typeof(Entity)));
		}
	}
}
