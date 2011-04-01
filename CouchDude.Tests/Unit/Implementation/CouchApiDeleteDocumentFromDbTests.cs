using CouchDude.Core;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Implementation
{
	public class CouchApiDeleteDocumentFromDbTests
	{
		[Fact]
		public void ShouldSendDeleteRequestOnDeletion()
		{
			var http = Mock.Of<IHttp>();
		}
	}
}