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


using CouchDude.Api;
using Moq;
using Xunit;

using CouchDude.Impl;
using CouchDude.Tests.SampleData;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class CouchSessionFlushTests
	{
		private readonly SimpleEntity entity = SimpleEntity.CreateStdWithoutRevision();

		[Fact]
		public void ShouldUpdateChangedDocumentsOnFlush()
		{
			IDocument lastUpdatedDoc = null;
			var totalSaveCount = 0;

			var couchApiMock = new Mock<ICouchApi>(MockBehavior.Loose);
			couchApiMock
				.Setup(ca => ca.SaveDocument(It.IsAny<IDocument>()))
				.Returns(
					(IDocument doc) => {
						lastUpdatedDoc = doc;
						totalSaveCount++;
						return new DocumentInfo(entity.Id, "2-1a517022a0c2d4814d51abfedf9bfee7").ToTask();
					});
			couchApiMock
				.Setup(ca => ca.Synchronously).Returns(() => new SynchronousCouchApi(couchApiMock.Object));
			
			var session = new CouchSession(Default.Settings, couchApiMock.Object);
			session.Save(entity);
			entity.Name = "Artem Tikhomirov";
			session.SaveChanges();

			Assert.Equal(2, totalSaveCount);
			Assert.Equal("simpleEntity.doc1", lastUpdatedDoc.Id);
			Assert.Equal(
				new {
					_id = "simpleEntity.doc1",
					_rev = "2-1a517022a0c2d4814d51abfedf9bfee7",
					type = "simpleEntity",
					name = "Artem Tikhomirov",
					age = 42,
					date = "1957-04-10T00:00:00"
				}.ToDocument(),
				lastUpdatedDoc
			);
		}
	}
}

