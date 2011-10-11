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
using CouchDude.Http;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Api
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
			HttpClientMock httpClientMock;
			var databaseApi = GetDbApi(HttpStatusCode.OK, "{\"ok\":true}", out httpClientMock);

			databaseApi.Synchronously.Delete();

			Assert.Equal(HttpMethod.Delete, httpClientMock.Request.Method);
			Assert.Null(httpClientMock.Request.Content);
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
			HttpClientMock httpClientMock;
			var databaseApi = GetDbApi(HttpStatusCode.OK, "{\"ok\":true}", out httpClientMock);

			databaseApi.Synchronously.Create();

			Assert.Equal(HttpMethod.Put, httpClientMock.Request.Method);
			Assert.Null(httpClientMock.Request.Content);
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
			HttpClientMock httpClientMock;
			var databaseApi = GetDbApi(HttpStatusCode.OK, SampleDbInfoString, out httpClientMock);

			databaseApi.Synchronously.RequestInfo();

			Assert.Equal(HttpMethod.Get, httpClientMock.Request.Method);
			Assert.Null(httpClientMock.Request.Content);
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
			HttpClientMock httpClientMock;
			var databaseApi = GetCouchApi(HttpStatusCode.OK, "[\"_replicator\",\"_users\",\"testdb\"]", out httpClientMock);
			databaseApi.Synchronously.RequestAllDbNames();

			Assert.Equal(HttpMethod.Get, httpClientMock.Request.Method);
			Assert.Equal("http://example.com:5984/_all_dbs", httpClientMock.Request.RequestUri.ToString());
			Assert.Null(httpClientMock.Request.Content);
		}

		[Fact]
		public void ShouldRetriveDatabaseList() 
		{
			HttpClientMock httpClientMock;
			var databaseApi = GetCouchApi(HttpStatusCode.OK, "[\"_replicator\",\"_users\",\"testdb\"]", out httpClientMock);
			databaseApi.Synchronously.RequestAllDbNames();

			Assert.Equal(HttpMethod.Get, httpClientMock.Request.Method);
			Assert.Equal("http://example.com:5984/_all_dbs", httpClientMock.Request.RequestUri.ToString());
			Assert.Null(httpClientMock.Request.Content);
		}

		private static IDatabaseApi GetDbApi(HttpStatusCode statusCode, string responseString)
		{
			HttpClientMock httpClientMock;
			return GetDbApi(statusCode, responseString, out httpClientMock);
		}

		private static IDatabaseApi GetDbApi(HttpStatusCode statusCode, string responseString, out HttpClientMock httpClientMock)
		{
			httpClientMock = new HttpClientMock(new HttpResponseMessage(statusCode) {
				Content = new StringContent(responseString)
			});
			return GetDbApi(httpClientMock);
		}

		private static IDatabaseApi GetDbApi(IHttpClient httpClient)
		{
			return GetCouchApi(httpClient).Db("testdb");
		}

		private static ICouchApi GetCouchApi(HttpStatusCode statusCode, string responseString, out HttpClientMock httpClientMock)
		{
			httpClientMock = new HttpClientMock(new HttpResponseMessage(statusCode) {
				Content = new StringContent(responseString)
			});
			return GetCouchApi(httpClientMock);
		}

		private static ICouchApi GetCouchApi(IHttpClient httpClient)
		{
			return Factory.CreateCouchApi(new Uri("http://example.com:5984/"), httpClient);
		}
	}
}