using CouchDude.Core.DesignDocumentManagment;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.DesignDocumentManagment
{
	public class DesignDocumentTests
	{
		[Fact]
		public void ShouldCopyWithRevisionProperly()
		{
			var json = JObject.Parse(@"{
				""_id"": ""_design/bin_doc1"",
				""some_property1"": ""test content""
			}");

			var document = new DesignDocument(json, "_design/bin_doc1", null);
			var copiedDocument = document.CopyWithRevision("3-ee7084f94345720bf9fdcd8f087e5518");

			Assert.Equal("_design/bin_doc1", copiedDocument.Id);
			Assert.Equal("3-ee7084f94345720bf9fdcd8f087e5518", copiedDocument.Revision);
			Assert.Equal(
				JObject.Parse(@"{
					""_id"": ""_design/bin_doc1"",
					""_rev"": ""3-ee7084f94345720bf9fdcd8f087e5518"",
					""some_property1"": ""test content""
				}"),
				copiedDocument.Definition,
				new JTokenEqualityComparer()
			);
		}
	}
}
