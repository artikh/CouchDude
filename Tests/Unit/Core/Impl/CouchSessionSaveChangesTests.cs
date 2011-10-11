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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class CouchSessionSaveChangesTests
	{
		[Fact]
		public void ShouldDelegateChangesSaveToApi()
		{
			var couchApi = Mock.Of<ICouchApi>(
				c => c.Db("testdb") == Mock.Of<IDatabaseApi>(
					d => d.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()) == 
						new Dictionary<string, DocumentInfo>().ToTask<IDictionary<string, DocumentInfo>>()
				)
			);

			ISession sesion = new CouchSession(Default.Settings, couchApi);
			sesion.Save(SimpleEntity.CreateStandardWithoutRevision());
			sesion.SaveChanges();
			
			Mock.Get(couchApi.Db("testdb")).Verify(dbApi => dbApi.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()), Times.Once());
		}

		[Fact]
		public void ShouldAssignNewlyUpdatedRevisionsToEntities() 
		{
			var couchApi = Mock.Of<ICouchApi>(
				c => c.Db("testdb") == Mock.Of<IDatabaseApi>(
					d => d.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()) ==
						new Dictionary<string, DocumentInfo> {
							{ SimpleEntity.StandardDocId, new DocumentInfo(SimpleEntity.StandardDocId, "2-cc2c5ab22cfa4a0faad27a0cb9ca7968") }
						}.ToTask<IDictionary<string, DocumentInfo>>()
				)
			);

			ISession sesion = new CouchSession(Default.Settings, couchApi);
			var entity = SimpleEntity.CreateStandardWithoutRevision();
			sesion.Save(entity);
			sesion.SaveChanges();

			Assert.Equal("2-cc2c5ab22cfa4a0faad27a0cb9ca7968", entity.Revision);
		}

		[Fact]
		public void ShouldNotAllowNewChangeSaveOperationBeforePreviousOneCompletes()
		{
			var apiShouldEnter = new AutoResetEvent(initialState: false);
			var bulkUpdateCalled = 0;

			IDictionary<string, DocumentInfo> returnInfo = new Dictionary<string, DocumentInfo> {
				{ SimpleEntity.StandardDocId, new DocumentInfo(SimpleEntity.StandardDocId, "2-cc2c5ab22cfa4a0faad27a0cb9ca7968") }
			};

			var dbApiMock = new Mock<IDatabaseApi>();
			dbApiMock
				.Setup(couchApi => couchApi.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()))
				.Returns<Action<IBulkUpdateBatch>>(
					updater => Task.Factory.StartNew(
						() => {

							apiShouldEnter.WaitOne();

							Interlocked.Increment(ref bulkUpdateCalled);
							updater(Mock.Of<IBulkUpdateBatch>());

							return returnInfo;
						},
						TaskCreationOptions.PreferFairness
					)
				);

			ISession sesion = new CouchSession(Default.Settings, Mock.Of<ICouchApi>(c => c.Db("testdb") == dbApiMock.Object));
			var entity = SimpleEntity.CreateStandardWithoutRevision();
			sesion.Save(entity);
			var firstSaveChangesTask = sesion.StartSavingChanges(); // Enters API method
			Thread.Sleep(500); //  Allowing first task to arrive to apiShouldEnter wating line first
			var secondSaveChangesTask = sesion.StartSavingChanges(); // Waits for first task to end

			Assert.Equal(0, bulkUpdateCalled);

			apiShouldEnter.Set();
			firstSaveChangesTask.WaitOrThrowOnTimeout();
			Assert.Equal(1, bulkUpdateCalled);


			entity.Age--;				    // Modifing entity for second save to be applicable
			apiShouldEnter.Set();   // Permitting second API method call to proceed
			secondSaveChangesTask.WaitOrThrowOnTimeout();
			Assert.Equal(2, bulkUpdateCalled);

			Assert.Equal("2-cc2c5ab22cfa4a0faad27a0cb9ca7968", entity.Revision);
		}

		[Theory]
		[InlineData("load")]
		[InlineData("query")]
		[InlineData("fullTextQuery")]
		public void ShouldNotAllowAnyOperationBeforeChangeSaveOperationCompletes(string operationName)
		{
			// I know this test is ginormus, but it's pretty self-contained and simple to understand

			var saveChangesOperationShouldProceed = new AutoResetEvent(initialState: false);
			var saveChangesOperationStarted = new AutoResetEvent(initialState: false);
			var executedOperations = new List<string>();

			IDictionary<string, DocumentInfo> returnInfo = new Dictionary<string, DocumentInfo> {
				{ SimpleEntity.StandardDocId, new DocumentInfo(SimpleEntity.StandardDocId, "2-cc2c5ab22cfa4a0faad27a0cb9ca7968") }
			};

			var dbApiMock = new Mock<IDatabaseApi>();
			dbApiMock
				.Setup(couchApi => couchApi.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()))
				.Returns<Action<IBulkUpdateBatch>>(
					updater => Task.Factory.StartNew(
						() => {
							saveChangesOperationStarted.Set();
							saveChangesOperationShouldProceed.WaitOne();
							updater(Mock.Of<IBulkUpdateBatch>());
							return returnInfo;
						}
					)
				);
			dbApiMock
				.Setup(api => api.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					_ => {
						lock (executedOperations)
							executedOperations.Add("Query");
						return ViewQueryResult.Empty.ToTask();
					});
			dbApiMock
				.Setup(api => api.QueryLucene(It.IsAny<LuceneQuery>()))
				.Returns<LuceneQuery>(
					_ => {
						lock (executedOperations)
							executedOperations.Add("QueryLucene");
						return
							LuceneQueryResult.Empty.ToTask();
					});
			dbApiMock
				.Setup(api => api.RequestDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string>(
					(_, __) => {
						lock (executedOperations)
							executedOperations.Add("RequestDocument");
						return SimpleEntity.CreateDocument().ToTask();
					});
			dbApiMock
				.Setup(api => api.RequestLastestDocumentRevision(It.IsAny<string>()))
				.Returns<string>(
					_ => {
						lock (executedOperations)
							executedOperations.Add("RequestLastestDocumentRevision");
						return SimpleEntity.StandardDocId.ToTask();
					});
			dbApiMock
				.Setup(api => api.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string>(
					(_, __) => {
						lock (executedOperations)
							executedOperations.Add("DeleteDocument");
						return SimpleEntity.StandardDocumentInfo.ToTask();
					});
			dbApiMock
				.Setup(api => api.SaveDocument(It.IsAny<IDocument>()))
				.Returns<IDocument>(
					_ => {
						lock (executedOperations)
							executedOperations.Add("SaveDocument");
						return SimpleEntity.StandardDocumentInfo.ToTask();
					});

			ISession sesion = new CouchSession(Default.Settings, Mock.Of<ICouchApi>(c => c.Db("testdb") == dbApiMock.Object));

			sesion.Save(SimpleEntity.CreateStandardWithoutRevision());
			var saveChangesOperation = sesion.StartSavingChanges();
			// Wating for StartSavingChanges to delegate execution to CouchAPI resetting session-wide wait handle
			saveChangesOperationStarted.WaitOne();

			var startOperationTask = Task.Factory.StartNew(
				() => {
					Task operation = null;
					switch (operationName)
					{
						case "load":
							operation = sesion.Load<SimpleEntity>("doc1a");
							break;
						case "query":
							operation =
								sesion.Query<SimpleEntity>(
									new ViewQuery {
										DesignDocumentName = "dd1",
										ViewName = "byX",
										Key = "key1",
										IncludeDocs = true
									});
							break;
						case "fullTextQuery":
							operation =
								sesion.QueryLucene<SimpleEntity>(
									new LuceneQuery
									{
										DesignDocumentName = "dd1",
										IndexName = "byX",
										IncludeDocs = true,
										Query = "key1:2"
									});
							break;
					}
					// ReSharper disable PossibleNullReferenceException
					operation.WaitOrThrowOnTimeout();
					// ReSharper restore PossibleNullReferenceException
				});

			Assert.True(
				executedOperations.Count == 0,
				string.Join(", ", executedOperations) + " operation(s) have executed before save changes operation completes");
			
			saveChangesOperationShouldProceed.Set();
			saveChangesOperation.WaitOrThrowOnTimeout();

			startOperationTask.WaitOrThrowOnTimeout();
			Assert.Equal(1, executedOperations.Count);
		}
	}
}
