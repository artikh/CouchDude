using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace CouchDude.Core.Configuration
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

		private const int IdsToPregenerate = 30000;
		private readonly ConcurrentQueue<string> pregeneratedIds = new ConcurrentQueue<string>();
		private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		private readonly Generator generator = new Generator();

		/// <inheritdoc/>
		public string GenerateId()
		{
			string generatedId;
			readerWriterLock.EnterUpgradeableReadLock();	
			try
			{
				if(!pregeneratedIds.TryDequeue(out generatedId))
				{
					try
					{
						readerWriterLock.EnterWriteLock();
						var i = IdsToPregenerate;
						while (--i > 0)
						{
							pregeneratedIds.Enqueue(generator.GetNext());
						}
						generatedId = generator.GetNext();
					}
					finally
					{
						readerWriterLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				readerWriterLock.ExitUpgradeableReadLock();
			}

			return generatedId;
		}
	}
}