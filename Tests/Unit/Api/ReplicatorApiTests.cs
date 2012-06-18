using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class ReplicatorApiTests
	{
		static ICouchApi GetCouchApi(string returnJsonString)
		{
			return GetCouchApi(new MockMessageHandler(returnJsonString, MediaType.Json));
		}

		static ICouchApi GetCouchApi(MockMessageHandler messageHandler = null)
		{
			return Factory.CreateCouchApi("http://example.com", handler: messageHandler ?? new MockMessageHandler());
		}

		[Fact]
		public void ShouldSaveNewReplicationDescriptor() 
		{
			var httpClientMock = new MockMessageHandler(
				new { id = "sourcedb_to_testdb", rev = "6-011f9010bf4edcb7312131b1d70fb060" }.ToJsonObject());
			GetCouchApi(httpClientMock).Replicator.Synchronously.SaveDescriptor(
				new ReplicationTaskDescriptor {
					Id = "sourcedb_to_testdb",
					Source = new Uri("http://example2.com/sourcedb"),
					Target = new Uri("testdb", UriKind.Relative),
					Continuous = true,
					CreateTarget = true
				});

			Assert.Equal(HttpMethod.Put, httpClientMock.Request.Method);
			Assert.Equal("http://example.com/_replicator/sourcedb_to_testdb", httpClientMock.Request.RequestUri.ToString());
			Assert.Equal(
				new {
					_id = "sourcedb_to_testdb", 
					target = "testdb",
					source = "http://example2.com/sourcedb", 
					continuous = true, 
					create_target = true
				}.ToJsonString(),
				httpClientMock.RequestBodyString
			);
		}

		[Fact]
		public void ShouldUpdateReplicationDescriptor() 
		{
			var httpClientMock = new MockMessageHandler(
				new { id = "sourcedb_to_testdb", rev = "7-011f9010bf4edcb7312131b1d70fb060" }.ToJsonObject());
			using (var couchApi = GetCouchApi(httpClientMock))
			{
				couchApi.Replicator.Synchronously.SaveDescriptor(
					new ReplicationTaskDescriptor {
						Id = "sourcedb_to_testdb",
						Revision = "6-011f9010bf4edcb7312131b1d70fb060",
						Target = new Uri("testdb", UriKind.Relative),
						Source = new Uri("http://example2.com/sourcedb"),
						Continuous = true,
						CreateTarget = true
					});

				Assert.Equal(HttpMethod.Put, httpClientMock.Request.Method);
				Assert.Equal(
					"http://example.com/_replicator/sourcedb_to_testdb",
					httpClientMock.Request.RequestUri.ToString());
				Assert.Equal(
					new {
						_id = "sourcedb_to_testdb",
						_rev = "6-011f9010bf4edcb7312131b1d70fb060",
						target = "testdb",
						source = "http://example2.com/sourcedb",
						continuous = true,
						create_target = true
					}.ToJsonString(),
					httpClientMock.RequestBodyString
					);
			}
		}

		[Fact]
		public void ShouldThrowOnNullOrInvalidInputOnSaveRequest()
		{
			var replicatorApi = GetCouchApi().Replicator.Synchronously;
			Assert.Throws<ArgumentNullException>(() => replicatorApi.SaveDescriptor(null));
			Assert.Throws<ArgumentException>(() => replicatorApi.SaveDescriptor(new ReplicationTaskDescriptor()));
			Assert.Throws<ArgumentException>(() => replicatorApi.SaveDescriptor(new ReplicationTaskDescriptor { Id = "" }));
		}

		[Fact]
		public void ShouldReturnNullIfNoDescriptorFoundLoadingById()
		{
			var httpClientMock = new MockMessageHandler(
				HttpStatusCode.NotFound, new { error = "not_found", reason = "missing" }.ToJsonObject());
			var descriptor = GetCouchApi(httpClientMock).Replicator.Synchronously.RequestDescriptorById("sourcedb_to_testdb");
			Assert.Null(descriptor);
		}

		[Fact]
		public void ShouldParseDescriptorGettingItById()
		{
			var httpClientMock = new MockMessageHandler(
				new {
					_id = "sourcedb_to_testdb",
					_rev = "6-011f9010bf4edcb7312131b1d70fb060",
					target = "http://example2.com/sourcedb",
					source = "testdb",
					continuous = true,
					create_target = true,
					_replication_state = "triggered",
					_replication_state_time = "2011-09-07T14:11:19+04:00",
					_replication_id = "cef1a54d4948bffcf9cb4d53591c5f5f"
				}.ToJsonObject());

			var descriptor = GetCouchApi(httpClientMock).Replicator.Synchronously.RequestDescriptorById("sourcedb_to_testdb");

			Assert.Equal("sourcedb_to_testdb", descriptor.Id);
			Assert.Equal("6-011f9010bf4edcb7312131b1d70fb060", descriptor.Revision);
			Assert.Equal("testdb", descriptor.Source.ToString());
			Assert.Equal("http://example2.com/sourcedb", descriptor.Target.ToString());
			Assert.True(descriptor.Continuous);
			Assert.True(descriptor.CreateTarget);
			Assert.Equal(ReplicationState.Triggered, descriptor.ReplicationState);
			Assert.Equal(new DateTime(2011, 9, 7, 10, 11, 19, DateTimeKind.Utc), descriptor.ReplicationStartTime);
			Assert.Equal("cef1a54d4948bffcf9cb4d53591c5f5f", descriptor.ReplicationId);
		}

		[Fact]
		public void ShouldRetriveReplicationDescriptorByIdViaGetRequest()
		{
			var httpClientMock = new MockMessageHandler(
				new {
					_id = "sourcedb_to_testdb",
					_rev = "6-011f9010bf4edcb7312131b1d70fb060",
					target = "http://example2.com/sourcedb",
					source = "testdb",
					continuous = true,
					create_target = true,
					_replication_state = "triggered",
					_replication_state_time = "2011-09-07T14:11:19+04:00",
					_replication_id = "cef1a54d4948bffcf9cb4d53591c5f5f"
				}.ToJsonObject());

			GetCouchApi(httpClientMock).Replicator.Synchronously.RequestDescriptorById("sourcedb_to_testdb");

			Assert.Equal(HttpMethod.Get, httpClientMock.Request.Method);
			Assert.Equal("http://example.com/_replicator/sourcedb_to_testdb", httpClientMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldThrowOnNullOrInvalidInputOnLoadRequest()
		{
			var replicatorApi = GetCouchApi().Replicator.Synchronously;
			Assert.Throws<ArgumentNullException>(() => replicatorApi.RequestDescriptorById(null));
			Assert.Throws<ArgumentNullException>(() => replicatorApi.RequestDescriptorById(string.Empty));
		}

		[Fact]
		public void ShouldRetriveReplicationDescriptorList()
		{
			var descriptorNames = GetCouchApi(@"{
					""total_rows"":5,
					""offset"":0,
					""rows"":[
						{""id"":""rep0"",""key"":""rep1"",""value"":{""rev"":""7-f245a7c9c32dcbd32fdf76b632626c51""}},
						{""id"":""rep1"",""key"":""rep2"",""value"":{""rev"":""1-cabb24990caa4fe70bc8adf9aba7d56f""}},
						{""id"":""rep2"",""key"":""rep3"",""value"":{""rev"":""2-c570644b63cb0417ed6d119a3df79c34""}},
						{""id"":""rep3"",""key"":""rep4"",""value"":{""rev"":""2-c570644b63cb0417ed6d119a3df79c34""}}
					]
				}").Replicator.Synchronously.GetAllDescriptorNames().ToList();

			Assert.Equal("rep0", descriptorNames[0]);
			Assert.Equal("rep1", descriptorNames[1]);
			Assert.Equal("rep2", descriptorNames[2]);
			Assert.Equal("rep3", descriptorNames[3]);
		}

		[Fact]
		public void ShouldSendGetRequestToAllDocsSpecialViewInOrderToRetriveReplicationDescriptorList()
		{
			var httpClientMock =
				new MockMessageHandler(@"{""total_rows"":0,""offset"":0,""rows"":[]}", MediaType.Json);

			GetCouchApi(httpClientMock).Replicator.Synchronously.GetAllDescriptorNames();

			Assert.Equal(HttpMethod.Get, httpClientMock.Request.Method);
			Assert.Equal("http://example.com/_replicator/_all_docs", httpClientMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldDeleteReplicationDescriptorSendingDeleteRequest()
		{
			var httpClientMock = new MockMessageHandler(
				new { id = "sourcedb_to_testdb", rev = "7-011f9010bf4edcb7312131b1d70fb060" }.ToJsonObject());

			GetCouchApi(httpClientMock).Replicator.Synchronously.Delete(new ReplicationTaskDescriptor {
				Id = "sourcedb_to_testdb",
				Revision = "6-011f9010bf4edcb7312131b1d70fb060",
				Target = new Uri("testdb", UriKind.Relative),
				Source = new Uri("http://example2.com/sourcedb"),
				Continuous = true,
				CreateTarget = true
			});

			Assert.Equal(HttpMethod.Delete, httpClientMock.Request.Method);
			Assert.Equal(
				"http://example.com/_replicator/sourcedb_to_testdb?rev=6-011f9010bf4edcb7312131b1d70fb060", 
				httpClientMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldThrowOnNullOrInvalidInputOnDeleteRequest() 
		{
			var replicatorApi = GetCouchApi().Replicator.Synchronously;
			Assert.Throws<ArgumentNullException>(() => replicatorApi.Delete(null));
			Assert.Throws<ArgumentException>(() => replicatorApi.Delete(new ReplicationTaskDescriptor()));
			Assert.Throws<ArgumentException>(() => replicatorApi.Delete(new ReplicationTaskDescriptor{ Id = "someId"}));
			Assert.Throws<ArgumentException>(() => replicatorApi.Delete(new ReplicationTaskDescriptor{ Revision = "someRev"}));
			Assert.Throws<ArgumentException>(() => replicatorApi.Delete(new ReplicationTaskDescriptor{ Id = "", Revision = "someRev"}));
			Assert.Throws<ArgumentException>(() => replicatorApi.Delete(new ReplicationTaskDescriptor{ Id = "someId", Revision = ""}));
		}
	}
}
