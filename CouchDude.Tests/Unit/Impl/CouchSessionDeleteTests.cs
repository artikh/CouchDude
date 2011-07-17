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
using CouchDude.Core.Api;
using Moq;
using Xunit;

using CouchDude.Core.Impl;
using CouchDude.Tests.SampleData;

namespace CouchDude.Tests.Unit.Impl
{
	public class CouchSessionDeleteTests
	{
		[Fact]
		public void ShouldInvokeCouchApiForDeletion()
		{
			var entity = SimpleEntity.CreateStd();
			string deletedId = null;
			string deletedRevision = null;

			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(ca => ca.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(
					(string id, string revision) => {
						deletedId = id;
						deletedRevision = revision;
						return new {ok = true, id, rev = "2-1a517022a0c2d4814d51abfedf9bfee7"}.ToJObject();
					});
			var session = new CouchSession(Default.Settings, couchApi.Object);
			var docInfo = session.Delete(entity);

			Assert.Equal(SimpleEntity.StandardDocId, deletedId);
			Assert.Equal(entity.Revision, deletedRevision);

			Assert.Equal(entity.Id, docInfo.Id);
			Assert.Equal("2-1a517022a0c2d4814d51abfedf9bfee7", docInfo.Revision);
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

			var couchApi = new Mock<ICouchApi>();
			couchApi
				.Setup(ca => ca.GetDocumentFromDbById(It.IsAny<string>()))
				.Returns(SimpleEntityWithoutRevision.DocumentWithRevision);
			couchApi
				.Setup(ca => ca.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns((string id, string rev) => {
					deletedId = id;
					deletedRev = rev;
					return SimpleEntityWithoutRevision.OkResponse;
				});

			var session = new CouchSession(Default.Settings, couchApi.Object);

			Assert.DoesNotThrow(() => {
				var entity = session.Load<SimpleEntityWithoutRevision>(SimpleEntityWithoutRevision.StandardEntityId);
				session.Delete(entity: entity);
			});

			Assert.Equal(SimpleEntityWithoutRevision.StandardDocId, deletedId);
			Assert.Equal(SimpleEntityWithoutRevision.StandardRevision, deletedRev);
		}
	}
}