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

using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class CouchSessionSaveTests
	{
		[Fact]
		public void ShouldThrowOnSaveWithRevision()
		{
			ISession session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			Assert.Throws<ArgumentException>(() => 
				session.Save(
					new Entity {
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
			ISession session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			Assert.Throws<ArgumentNullException>(() => session.Save(null as Entity));
			Assert.Throws<ArgumentNullException>(() => session.Save(null as Entity[]));
			Assert.Throws<ArgumentNullException>(() => session.Save(Entity.CreateStandardWithoutRevision(), null));
		}

		[Fact]
		public void ShouldGenerateIdIfNoneSet()
		{
			var savingEntity = new Entity { Name = "John Smith" };
			ISession session = new CouchSession(Default.Settings, Mock.Of<ICouchApi>());
			session.Save(savingEntity);

			Assert.NotNull(savingEntity.Id);
			Assert.NotEqual(string.Empty, savingEntity.Id);
		}

		[Fact]
		public void ShouldCacheFreashlySavedInstance() 
		{
			var savingEntity = Entity.CreateStandardWithoutRevision();
			ISession session = new CouchSession(
				Default.Settings, 
				Mock.Of<ICouchApi>(c => c.Db("testdb") == new Mock<IDatabaseApi>(MockBehavior.Strict).Object));
			session.Save(savingEntity);

			var loadedInstance = session.Synchronously.Load<Entity>(savingEntity.Id);
			Assert.Same(savingEntity, loadedInstance);
		}
	}
}