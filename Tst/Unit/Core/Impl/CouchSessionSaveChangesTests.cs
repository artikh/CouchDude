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
			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
				.Setup(couchApi => couchApi.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()))
				.Returns(new Dictionary<string, DocumentInfo>().ToTask<IDictionary<string, DocumentInfo>>());

			ISession sesion = new CouchSession(Default.Settings, couchApiMock.Object);
			sesion.Save(SimpleEntity.CreateStandardWithoutRevision());
			sesion.SaveChanges();
			
			couchApiMock.Verify(couchApi => couchApi.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()), Times.Once());
		}

		[Fact]
		public void ShouldAssignNewlyUpdatedRevisionsToEntities() 
		{
			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
				.Setup(couchApi => couchApi.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()))
				.Returns(
					new Dictionary<string, DocumentInfo> {
						{ SimpleEntity.StandardDocId, new DocumentInfo(SimpleEntity.StandardDocId, "2-cc2c5ab22cfa4a0faad27a0cb9ca7968") }
					}.ToTask<IDictionary<string, DocumentInfo>>());

			ISession sesion = new CouchSession(Default.Settings, couchApiMock.Object);
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

			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
				.Setup(couchApi => couchApi.BulkUpdate(It.IsAny<Action<IBulkUpdateBatch>>()))
				.Returns<Action<IBulkUpdateBatch>>(
					updater => Task.Factory.StartNew(
						() => {

							apiShouldEnter.WaitOne();

							Interlocked.Increment(ref bulkUpdateCalled);
							updater(Mock.Of<IBulkUpdateBatch>());

							return returnInfo;
						}
					)
				);

			ISession sesion = new CouchSession(Default.Settings, couchApiMock.Object);
			var entity = SimpleEntity.CreateStandardWithoutRevision();
			sesion.Save(entity);
			var firstSaveChangesTask = sesion.StartSavingChanges(); // Enters API method
			var secondSaveChangesTask = sesion.StartSavingChanges(); // Waits for first task to end

			Assert.Equal(0, bulkUpdateCalled);

			apiShouldEnter.Set();
			firstSaveChangesTask.Wait();
			Assert.Equal(1, bulkUpdateCalled);


			entity.Age--;				    // Modifing entity for second save to be applicable
			apiShouldEnter.Set();   // Permitting second API method call to proceed
			secondSaveChangesTask.Wait();
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

			var couchApiMock = new Mock<ICouchApi>();
			couchApiMock
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
			couchApiMock
				.Setup(api => api.Query(It.IsAny<ViewQuery>()))
				.Returns<ViewQuery>(
					_ => {
						lock (executedOperations)
							executedOperations.Add("Query");
						return ViewQueryResult.Empty.ToTask();
					});
			couchApiMock
				.Setup(api => api.QueryLucene(It.IsAny<LuceneQuery>()))
				.Returns<LuceneQuery>(
					_ =>
					{
						lock (executedOperations)
							executedOperations.Add("QueryLucene");
						return
							LuceneQueryResult.Empty.ToTask();
					});
			couchApiMock
				.Setup(api => api.RequestDocumentById(It.IsAny<string>()))
				.Returns<string>(
					_ =>
					{
						lock (executedOperations)
							executedOperations.Add("RequestDocumentById");
						return SimpleEntity.CreateDocument().ToTask();
					});
			couchApiMock
				.Setup(api => api.RequestLastestDocumentRevision(It.IsAny<string>()))
				.Returns<string>(
					_ =>
					{
						lock (executedOperations)
							executedOperations.Add("RequestLastestDocumentRevision");
						return SimpleEntity.StandardDocId.ToTask();
					});
			couchApiMock
				.Setup(api => api.DeleteDocument(It.IsAny<string>(), It.IsAny<string>()))
				.Returns<string, string>(
					(_, __) =>
					{
						lock (executedOperations)
							executedOperations.Add("DeleteDocument");
						return SimpleEntity.StandardDocumentInfo.ToTask();
					});
			couchApiMock
				.Setup(api => api.SaveDocument(It.IsAny<IDocument>()))
				.Returns<IDocument>(
					_ =>
					{
						lock (executedOperations)
							executedOperations.Add("SaveDocument");
						return SimpleEntity.StandardDocumentInfo.ToTask();
					});

			ISession sesion = new CouchSession(Default.Settings, couchApiMock.Object);

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
					operation.Wait();
					// ReSharper restore PossibleNullReferenceException
				});

			Assert.True(
				executedOperations.Count == 0,
				string.Join(", ", executedOperations) + " operation(s) have executed before save changes operation completes");
			
			saveChangesOperationShouldProceed.Set();
			saveChangesOperation.Wait();

			startOperationTask.Wait();
			Assert.Equal(1, executedOperations.Count);
		}
	}
}
