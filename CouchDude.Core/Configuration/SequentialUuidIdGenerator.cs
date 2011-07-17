#region Licence Info 
/*
  Copyright 2011 · Artem Tikhomirov																					
 																																					
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