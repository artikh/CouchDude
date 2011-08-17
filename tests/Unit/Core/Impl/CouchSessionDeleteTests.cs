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
using Moq;
using Xunit;

using CouchDude.Impl;
using CouchDude.Tests.SampleData;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class CouchSessionDeleteTests
	{
		[Fact]
		public void ShouldInvokeCouchApiForDeletion()
		{
			var entity = SimpleEntity.CreateStd();
			string deletedId = null;
			string deletedRevision = null;

			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
				.Setup(ca => ca.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(
					(string id, string revision) => {
						deletedId = id;
						deletedRevision = revision;
						return new DocumentInfo(id, "2-1a517022a0c2d4814d51abfedf9bfee7").ToTask();
					});
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(new SynchronousCouchApi(couchApiMock.Object));
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Delete(entity);

			Assert.Equal(SimpleEntity.StandardDocId, deletedId);
			Assert.Equal(entity.Revision, deletedRevision);
		}

		[Fact]
		public void ShouldThrowOnNullEntity()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Delete<SimpleEntity>(entity: null));
		}

		[Fact]
		public void ShouldThrowArgumentExceptionIfNoRevisionOnNewEntity()
		{
			Assert.Throws<ArgumentException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Delete(entity: SimpleEntity.CreateStdWithoutRevision())
			);
		}

		[Fact]
		public void ShouldNotThrowArgumentExceptionIfNoRevisionOnLoadedEntity()
		{
			string deletedId = null;
			string deletedRev = null;

			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
				.Setup(ca => ca.RequestDocumentById(It.IsAny<string>()))
				.Returns(SimpleEntityWithoutRevision.DocWithRevision.ToTask());
			couchApiMock
				.Setup(ca => ca.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns((string id, string rev) => {
					deletedId = id;
					deletedRev = rev;
					return SimpleEntityWithoutRevision.OkResponse.ToTask();
				});
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(new SynchronousCouchApi(couchApiMock.Object));

			var session = new CouchSession(Default.Settings, couchApiMock.Object);

			Assert.DoesNotThrow(() => {
				var entity = session.Synchronously.Load<SimpleEntityWithoutRevision>(SimpleEntityWithoutRevision.StandardEntityId);
				session.Delete(entity: entity);
			});

			Assert.Equal(SimpleEntityWithoutRevision.StandardDocId, deletedId);
			Assert.Equal(SimpleEntityWithoutRevision.StandardRevision, deletedRev);
		}
	}
}