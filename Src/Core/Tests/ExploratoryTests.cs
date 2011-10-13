#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov, Stas Girkin, Mikhail Anikeev-Naumenko																					
																																					
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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;


// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local
#pragma warning disable 649

namespace CouchDude.Tests
{
	public class ExploratoryTests
	{
		private const string SomeNestedJson =
			@"{
				""str"": ""some string"",
				""number"": 42,
				""boolean"": true,
				""array"": [
					""some strange string"",
					42,
					true
				],
				""object"": {
					""str"": ""some string"",
					""number"": 42,
					""boolean"": true
				}
			}";

		private const string SomeAltNestedJson =
			@"{
				""str"": ""some string"",
				""number"": 42,
				""boolean"": true,
				""array"": [
					""some strange string"",
					42,
					true
				],
				""object"": {
					""str"": ""some string"",
					""number"": 42,
					""boolean"": false
				}
			}";

		[Fact]
		public void SouldCompareTwoJObjectsCorrectly()
		{
			var obj1 = JObject.Parse(SomeNestedJson);
			var obj2 = JObject.Parse(SomeNestedJson + "   ");

			Assert.True(new JTokenEqualityComparer().Equals(obj1, obj2));
		}

		[Fact]
		public void ShouldDetectDeepNestedDifferences()
		{
			var obj1 = JObject.Parse(SomeNestedJson);
			var obj2 = JObject.Parse(SomeAltNestedJson);

			Assert.False(new JTokenEqualityComparer().Equals(obj1, obj2));
		}

		private class ClassWithJObjectProperty
		{
			public string Id;
			public IList<JObject> SubObject;
		}

		public class PrivatePropertySetterClass
		{
			private string name = "name name";

			public PrivatePropertySetterClass(string name, int age)
			{
				this.name = name;
				Age = age;
			}

			public string Name { get { return name; } private set { name = value; } }

			public int Age { get; private set; }
		}

		[Fact]
		public void ShouldSuppressUnescaping()
		{
			var uri = new Uri("http://www.example.com/value%2fvalue");
			uri.LeaveDotsAndSlashesEscaped();

			Assert.Equal("http://www.example.com/value%2fvalue", uri.ToString());
		}

		[Fact]
		public void ShouldWaitTillChildTaskFinishes()
		{
			var stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			Thread.Sleep(1);
			long innerTaskContinuationFishTime = 0;
			var task = Task.Factory.StartNew(
				() => {
					Task.Factory
						.StartNew(() => Thread.Sleep(1000))
						.ContinueWith(
							_ => {
								Thread.Sleep(1000);
								innerTaskContinuationFishTime = stopwatch.ElapsedTicks;
							},
							TaskContinuationOptions.AttachedToParent);
				});
			task.WaitOrThrowOnTimeout();

			Assert.True(innerTaskContinuationFishTime > 0);
		}
	}


public static class UriFix
	{
		private const int UnEscapeDotsAndSlashes = 0x2000000;
		private const int SimpleUserSyntax = 0x20000;

		public static void LeaveDotsAndSlashesEscaped(this Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			FieldInfo fieldInfo = uri.GetType().GetField("m_Syntax", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Syntax' field not found");

			object uriParser = fieldInfo.GetValue(uri);
			fieldInfo = typeof (UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo == null)
				throw new MissingFieldException("'m_Flags' field not found");

			object uriSyntaxFlags = fieldInfo.GetValue(uriParser);

			// Clear the flag that we don't want
			uriSyntaxFlags = (int) uriSyntaxFlags & ~UnEscapeDotsAndSlashes;
			uriSyntaxFlags = (int) uriSyntaxFlags & ~SimpleUserSyntax;
			fieldInfo.SetValue(uriParser, uriSyntaxFlags);
		}
	}
}