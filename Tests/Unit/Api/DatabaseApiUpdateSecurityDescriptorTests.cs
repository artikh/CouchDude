using System.Net;
using System.Net.Http;
using CouchDude.Utils;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class DatabaseApiUpdateSecurityDescriptorTests
	{
		[Fact]
		public void ShouldThrowIfDatabaseMissing()
		{
			var httpClient = new MockMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound) {
				Content = new JsonContent("{\"error\":\"not_found\",\"reason\":\"no_db_file\"}")
			});
			Assert.Throws<DatabaseMissingException>(
				() => CreateCouchApi(httpClient).Db("testdb").Synchronously.UpdateSecurityDescriptor(new DatabaseSecurityDescriptor())
			);
		}

		[Fact]
		public void ShouldThrowOnBadRequest()
		{
			var httpClient = new MockMessageHandler(new HttpResponseMessage(HttpStatusCode.BadRequest) {
				Content = new JsonContent("{\"error\":\"bad_request\",\"reason\":\"unknown error\"}")
			});
			Assert.Throws<CouchCommunicationException>(
				() => CreateCouchApi(httpClient).Db("testdb").Synchronously.UpdateSecurityDescriptor(new DatabaseSecurityDescriptor())
			);
		}

		[Fact]
		public void ShouldMakePutRequest()
		{
			var httpClient = new MockMessageHandler(
				new HttpResponseMessage(HttpStatusCode.OK) { Content = new JsonContent("{\"ok\": true}")});

			CreateCouchApi(httpClient).Db("testdb").Synchronously.UpdateSecurityDescriptor(new DatabaseSecurityDescriptor {
				Admins = new NamesRoles { Names = new[]{"admin1", "admin2"}, Roles = new[]{"admins1", "admins2"}},
				Readers = new NamesRoles { Names = new[]{"reader1", "reader2"}, Roles = new[]{"readers1", "readers2"}},
			});

			Assert.Equal(HttpMethod.Put, httpClient.Request.Method);
			TestUtils.AssertSameJson(
				new {
					admins = new {names = new[]{"admin1", "admin2"}, roles = new[]{"admins1", "admins2"}},
					readers = new { names = new[] { "reader1", "reader2" }, roles = new[] { "readers1", "readers2" } }
				}, 
				httpClient.RequestBodyString);
		}

		private static ICouchApi CreateCouchApi(MockMessageHandler handler = null)
		{
			return Factory.CreateCouchApi("http://example.com:5984/", handler);
		}
	}
}
