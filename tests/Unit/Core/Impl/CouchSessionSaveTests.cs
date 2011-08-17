#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
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

using CouchDude.Api;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
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
			var couchApiMock = MockCouchApi();
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Save(entity);
			Assert.Throws<ArgumentException>(() => session.Save(entity));
		}

		[Fact]
		public void ShouldThrowOnSaveWithRevision()
		{
			var couchApiMock = MockCouchApi();
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			Assert.Throws<ArgumentException>(() => 
				session.Save(
					new SimpleEntity {
						Id = "doc1",
						Revision = "42-1a517022a0c2d4814d51abfedf9bfee7",
						Name = "John Smith"
					}
				)
			);
		}

		[Fact]
		public void ShouldThrowOnNullEntity()
		{
			var session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			Assert.Throws<ArgumentNullException>(() => session.Save<SimpleEntity>(null));
		}

		[Fact]
		public void ShouldReturnFillRevisionPropertyOnEntity()
		{
			var couchApiMock = MockCouchApi();
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Save(entity);
			Assert.Equal("42-1a517022a0c2d4814d51abfedf9bfee7", entity.Revision);
		}

		[Fact]
		public void ShouldAssignOnSaveIfNoneWasAssignedBefore()
		{
			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.SaveDocument(It.IsAny<IDocument>()))
				.Returns(
					(IDocument doc) => new { id = doc.Id, rev = "1-1a517022a0c2d4814d51abfedf9bfee7" }.ToJsonFragment().ToTask()
				);
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(() => new SynchronousCouchApi(couchApiMock.Object));

			var savingEntity = new SimpleEntity
			{
				Name = "John Smith"
			};
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Save(savingEntity);

			Assert.NotNull(savingEntity.Id);
			Assert.NotEqual(string.Empty, savingEntity.Id);
		}

		private static Mock<ICouchApi> MockCouchApi()
		{
			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.SaveDocument(It.IsAny<Document>()))
				.Returns(new {
					id = SimpleEntity.StandardDocId,
					rev = "42-1a517022a0c2d4814d51abfedf9bfee7"
				}.ToJsonFragment().ToTask());
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(() => new SynchronousCouchApi(couchApiMock.Object));
			return couchApiMock;
		}
	}
}