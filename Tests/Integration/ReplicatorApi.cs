using System;
using System.Linq;
using System.Threading.Tasks;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class ReplicatorApi
	{
		[Fact]
		public async Task ShouldFetchSaveAndDeleteReplicationDescriptors()
		{
			var replicatorDbName = "test_replicator_" + Guid.NewGuid();

			var couchApi = (ICouchApi) new CouchApi(new CouchApiSettings("http://127.0.0.1:5984/"){ ReplicatorDatabase = replicatorDbName});

			await TaskEx.WhenAll(
				couchApi.Db(replicatorDbName).Create(throwIfExists: false),
				couchApi.Db("target__a").Create(throwIfExists: false),
				couchApi.Db("target__b").Create(throwIfExists: false),
				couchApi.Db("source_a").Create(throwIfExists: false),
				couchApi.Db("source_b").Create(throwIfExists: false)
			);

			var replicatorApi = couchApi.Replicator;

			Assert.Equal(0, (await replicatorApi.GetAllDescriptorNames()).Count);
			Assert.Equal(0, (await replicatorApi.GetAllDescriptors()).Count);

			await replicatorApi.SaveDescriptor(new ReplicationTaskDescriptor {
				Id = "A",
				Continuous = true, 
				Source = new Uri("source_a", UriKind.Relative),
				Target = new Uri("target_a", UriKind.Relative),
			});
			await replicatorApi.SaveDescriptor(new ReplicationTaskDescriptor {
				Id = "B",
				Continuous = true, 
				Source = new Uri("source_b", UriKind.Relative),
				Target = new Uri("target_b", UriKind.Relative),
			});

			Assert.Equal(2, (await replicatorApi.GetAllDescriptorNames()).Count);
			Assert.Equal(2, (await replicatorApi.GetAllDescriptors()).Count);

			Assert.Equal(new[]{ "A", "B" }, (await replicatorApi.GetAllDescriptorNames()));
			var descriptors = await replicatorApi.GetAllDescriptors();

			var descriptorA = descriptors.First();
			Assert.Equal("A", descriptorA.Id);
			Assert.Equal("source_a", descriptorA.Source.ToString());
			Assert.Equal("target_a", descriptorA.Target.ToString());
			Assert.True(descriptorA.Continuous);

			var descriptorB = descriptors.Skip(1).First();
			Assert.Equal("B", descriptorB.Id);
			Assert.Equal("source_b", descriptorB.Source.ToString());
			Assert.Equal("target_b", descriptorB.Target.ToString());
			Assert.True(descriptorB.Continuous);

			await replicatorApi.DeleteDescriptor(descriptorA);
			Assert.Equal(1, (await replicatorApi.GetAllDescriptorNames()).Count);
			Assert.Equal(1, (await replicatorApi.GetAllDescriptors()).Count);

			await replicatorApi.DeleteDescriptor(descriptorB);
			Assert.Equal(0, (await replicatorApi.GetAllDescriptorNames()).Count);
			Assert.Equal(0, (await replicatorApi.GetAllDescriptors()).Count);
		}
	}
}