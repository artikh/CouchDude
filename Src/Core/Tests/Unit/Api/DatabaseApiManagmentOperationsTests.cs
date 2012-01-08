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
using System.Net;
using System.Net.Http;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiManagmentOperationsTests
	{
		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var databaseApi = GetDbApi(HttpStatusCode.NotFound, "{\"error\":\"not_found\",\"reason\":\"no_db_file\"}");

			Assert.Throws<DatabaseMissingException>(() => databaseApi.Synchronously.Delete());
		}

		[Fact]
		public void ShouldSendDeleteRequestToDatabase()
		{
			MockMessageHandler handler;
			var databaseApi = GetDbApi(HttpStatusCode.OK, "{\"ok\":true}", out handler);

			databaseApi.Synchronously.Delete();

			Assert.Equal(HttpMethod.Delete, handler.Request.Method);
			Assert.Null(handler.Request.Content);
		}

		[Fact]
		public void ShouldThrowCommunicationExceptionOnErrorDeleting()
		{
			var databaseApi = GetDbApi(
				HttpStatusCode.InternalServerError, "{\"error\":\"some obscure error\",\"reason\":\"none of your business\"}");

			Assert.Throws<CouchCommunicationException>(() => databaseApi.Synchronously.Delete());
		}

		[Fact]
		public void ShouldPutRequestToDatabase()
		{
			MockMessageHandler mockMessageHandler;
			var databaseApi = GetDbApi(HttpStatusCode.OK, "{\"ok\":true}", out mockMessageHandler);

			databaseApi.Synchronously.Create();

			Assert.Equal(HttpMethod.Put, mockMessageHandler.Request.Method);
			Assert.Null(mockMessageHandler.Request.Content);
		}

		[Fact]
		public void ShouldThrowIfDatabaseAlreadyExists()
		{
			var databaseApi = GetDbApi(
				HttpStatusCode.PreconditionFailed, 
				"{\"error\":\"file_exists\",\"reason\":\"The database could not be created, the file already exists.\"}");

			Assert.Throws<CouchCommunicationException>(() => databaseApi.Synchronously.Create());
		}

		[Fact]
		public void ShouldThrowCommunicationExceptionOnErrorCreating()
		{
			var databaseApi = GetDbApi(
				HttpStatusCode.InternalServerError, "{\"error\":\"some obscure error\",\"reason\":\"none of your business\"}");

			Assert.Throws<CouchCommunicationException>(() => databaseApi.Synchronously.Create());
		}

		[Fact]
		public void ShouldThrowCommunicationExceptionOnErrorGettingInfo()
		{
			var databaseApi = GetDbApi(
				HttpStatusCode.InternalServerError, "{\"error\":\"some obscure error\",\"reason\":\"none of your business\"}");

			Assert.Throws<CouchCommunicationException>(() => databaseApi.Synchronously.RequestInfo());
		}

		private const string SampleDbInfoString =
			"{\"db_name\":\"testdb\",\"doc_count\":670,\"doc_del_count\":167,\"update_seq\":1043," +
			 "\"purge_seq\":1,\"compact_running\":true,\"disk_size\":1622114,\"instance_start_time\":\"1315299685776000\"," +
			 "\"disk_format_version\":5,\"committed_update_seq\":1043}";

		[Fact]
		public void ShouldSendGetRequestForInfo()
		{
			MockMessageHandler mockMessageHandler;
			var databaseApi = GetDbApi(HttpStatusCode.OK, SampleDbInfoString, out mockMessageHandler);

			databaseApi.Synchronously.RequestInfo();

			Assert.Equal(HttpMethod.Get, mockMessageHandler.Request.Method);
			Assert.Null(mockMessageHandler.Request.Content);
		}

		[Fact]
		public void ShouldCorrectlyParseDatabaseInfo()
		{
			var databaseApi = GetDbApi(HttpStatusCode.OK, SampleDbInfoString);

			var dbInfo = databaseApi.Synchronously.RequestInfo();

			Assert.Equal("testdb", dbInfo.Name);
			Assert.True(dbInfo.Exists);
			Assert.Equal(670, dbInfo.DocumentCount);
			Assert.Equal(1043, dbInfo.UpdateSequenceNumber);
			Assert.Equal(1, dbInfo.PurgeOperationsPerformed);
			Assert.True(dbInfo.DoesCompactionRunning);
			Assert.Equal(1622114, dbInfo.FileSizeInBytes);
			Assert.Equal(5, dbInfo.FileFormatVersion);
		}

		[Fact]
		public void ShouldReturnProperDbInfoIfDbDoesNotExist()
		{
			var databaseApi = GetDbApi(HttpStatusCode.NotFound, "{\"error\":\"not_found\",\"reason\":\"no_db_file\"}");

			var dbInfo = databaseApi.Synchronously.RequestInfo();

			Assert.Equal("testdb", dbInfo.Name);
			Assert.False(dbInfo.Exists);
		}

		[Fact]
		public void ShouldRetriveDatabaseListUsingGetRequest() 
		{
			MockMessageHandler mockMessageHandler;
			var databaseApi = GetCouchApi(HttpStatusCode.OK, "[\"_replicator\",\"_users\",\"testdb\"]", out mockMessageHandler);
			databaseApi.Synchronously.RequestAllDbNames();

			Assert.Equal(HttpMethod.Get, mockMessageHandler.Request.Method);
			Assert.Equal("http://example.com:5984/_all_dbs", mockMessageHandler.Request.RequestUri.ToString());
			Assert.Null(mockMessageHandler.Request.Content);
		}

		[Fact]
		public void ShouldRetriveDatabaseList() 
		{
			MockMessageHandler mockMessageHandler;
			var databaseApi = GetCouchApi(HttpStatusCode.OK, "[\"_replicator\",\"_users\",\"testdb\"]", out mockMessageHandler);
			databaseApi.Synchronously.RequestAllDbNames();

			Assert.Equal(HttpMethod.Get, mockMessageHandler.Request.Method);
			Assert.Equal("http://example.com:5984/_all_dbs", mockMessageHandler.Request.RequestUri.ToString());
			Assert.Null(mockMessageHandler.Request.Content);
		}

		private static IDatabaseApi GetDbApi(HttpStatusCode statusCode, string responseJson)
		{
			MockMessageHandler mockMessageHandler;
			return GetDbApi(statusCode, responseJson, out mockMessageHandler);
		}

		private static IDatabaseApi GetDbApi(HttpStatusCode statusCode, string responseJson, out MockMessageHandler handler)
		{
			handler = new MockMessageHandler(new HttpResponseMessage(statusCode) {
				Content = new JsonContent(responseJson)
			});
			return GetDbApi(handler);
		}

		private static IDatabaseApi GetDbApi(MockMessageHandler handler)
		{
			return GetCouchApi(handler).Db("testdb");
		}

		private static ICouchApi GetCouchApi(HttpStatusCode statusCode, string responseJson, out MockMessageHandler handler)
		{
			handler = new MockMessageHandler(new HttpResponseMessage(statusCode){
				Content = new JsonContent(responseJson)
			});
			return GetCouchApi(handler);
		}

		private static ICouchApi GetCouchApi(MockMessageHandler handler)
		{
			return Factory.CreateCouchApi(new Uri("http://example.com:5984/"), handler);
		}
	}
}