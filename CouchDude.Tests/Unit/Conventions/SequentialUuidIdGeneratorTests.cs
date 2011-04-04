using System;
using System.Collections.Generic;
using CouchDude.Core.Conventions;
using Xunit;

namespace CouchDude.Tests.Unit.Conventions
{
	public class SequentialUuidIdGeneratorTests
	{
		[Fact]
		public void ShouldGenerateUuidsStartingWithTheSame13Chars()
		{
			var generator = new SequentialUuidIdGenerator();
			Assert.Equal(generator.GenerateId().Substring(0, 13), generator.GenerateId().Substring(0, 13));
		}

		[Fact]
		public void ShouldGenerateDifferentUuids()
		{
			var generator = new SequentialUuidIdGenerator();
			var loops = 1000;
			var generatedUuids = new HashSet<string>();
			for (var i = 0; i < loops; i++)
			{
				var uuid = generator.GenerateId();
				Assert.False(generatedUuids.Contains(uuid));
				generatedUuids.Add(uuid);
			}
		}
	}
}