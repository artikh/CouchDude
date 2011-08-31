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
using System.Collections.Generic;
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
		public void ShouldStopReturnigDeletedEntityFromCache()
		{
			var entity = Entity.CreateStandardWithoutRevision();
			ISession session = new CouchSession(Default.Settings, new Mock<ICouchApi>(MockBehavior.Strict).Object);
			session.Save(entity);
			session.Delete(entity);

			Assert.Null(session.Synchronously.Load<Entity>(entity.Id));
		}

		[Fact]
		public void ShouldThrowOnNullEntity()
		{
			Assert.Throws<ArgumentNullException>(
				() => new CouchSession(Default.Settings, Mock.Of<ICouchApi>()).Delete<Entity>(entity: null));
		}

		[Fact]
		public void ShouldNotThrowArgumentExceptionIfNoRevisionOnLoadedEntity()
		{
			string deletedId = null;
			string deletedRev = null;

			var bulkUpdateBatchMock = new Mock<IBulkUpdateBatch>();
			bulkUpdateBatchMock
				.Setup(b => b.Delete(It.IsAny<string>(), It.IsAny<string>()))
				.Callback(
					(string id, string rev) => {
						deletedId = id;
						deletedRev = rev;
					});

			var documentInfo = new DocumentInfo(EntityWithoutRevision.StandardDocId, "2-1a517022a0c2d4814d51abfedf9bfee7");

			DocumentInfo result = documentInfo;
			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
				.Setup(ca => ca.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()))
				.Returns<Action<IBulkUpdateBatch>>(
					updateAction => {
						updateAction(bulkUpdateBatchMock.Object);
						return new Dictionary<string, DocumentInfo> {{result.Id, result}}
							.ToTask<IDictionary<string, DocumentInfo>>();
					});
			couchApiMock
				.Setup(ca => ca.RequestDocumentById(It.IsAny<string>()))
				.Returns(EntityWithoutRevision.CreateDocumentWithRevision().ToTask());
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(new SynchronousCouchApi(couchApiMock.Object));

			var session = new CouchSession(Default.Settings, couchApiMock.Object);

			Assert.DoesNotThrow(() => {
				var entity = session.Synchronously.Load<EntityWithoutRevision>(EntityWithoutRevision.StandardEntityId);
				session.Delete(entity: entity);
				session.SaveChanges();
			});

			Assert.Equal(EntityWithoutRevision.StandardDocId, deletedId);
			Assert.Equal(EntityWithoutRevision.StandardRevision, deletedRev);
		}
		
		private static ICouchApi MockCouchApi(DocumentInfo result, Mock<IBulkUpdateBatch> bulkUpdateBatchMock)
		{
			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
				.Setup(ca => ca.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()))
				.Returns<Action<IBulkUpdateBatch>>(
					updateAction => {
						updateAction(bulkUpdateBatchMock.Object);
						return new Dictionary<string, DocumentInfo> {{result.Id, result}}
							.ToTask<IDictionary<string, DocumentInfo>>();
					});
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(new SynchronousCouchApi(couchApiMock.Object));
			var couchApi = couchApiMock.Object;
			return couchApi;
		}
	}
}