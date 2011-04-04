using System;
using System.Text;

namespace CouchDude.Core.Conventions
{
	/// <summary>Generates UUIDs using algorithm similar to CouchBD's 'sequence'
	/// algorithm.</summary>
	public class SequentialUuidIdGenerator: IIdGenerator
	{
		internal class Generator
		{
			private const int MaxSuffixValue = 16777216; // 16^6
			private const int MinimumIncrement = 1;
			private const int MaximumIncrement = 15;
			private const int PrefixLength = 13;

			private string currentPrefix;
			private int currentSuffix;
			private readonly Random randomGenerator = new Random();

			public Generator()
			{
				GenerateNewPrefix();
				Increment();
			}

			public string GetNext()
			{
				Increment();
				if (currentSuffix >= MaxSuffixValue)
				{
					GenerateNewPrefix();
					Increment();
				}

				return currentPrefix + currentSuffix.ToString("x6");
			}

			private void GenerateNewPrefix()
			{
				var bytes = Guid.NewGuid().ToByteArray();
				var stringBuilder = new StringBuilder(bytes.Length*2);
				for (var i = 0; i < PrefixLength; i++)
					stringBuilder.AppendFormat("{0:x2}", bytes[i]);
				currentPrefix = stringBuilder.ToString();
			}

			private void Increment()
			{
				currentSuffix += randomGenerator.Next(MinimumIncrement, MaximumIncrement);
			}
		}

		[ThreadStatic]
		private Generator generator;

		/// <inheritdoc/>
		public string GenerateId()
		{
			if(generator == null)
				generator = new Generator();
			return generator.GetNext();
		}
	}
}