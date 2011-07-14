using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouchDude.Core;
using Xunit;

namespace CouchDude.Tests.Unit
{
	public class DocumentTests
	{
		[Fact]
		public void ShouldAccessRevisionViaSpecialProperty()
		{
			var document = new Document();
			((dynamic) document)._rev = "1-42";

			Assert.Equal("1-42", document.Revision);
		}
	}
}
