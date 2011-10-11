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

using Xunit;

namespace CouchDude.Tests.Unit.Core
{
	public class SettingsTests
	{
		[Fact]
		public void ShouldThrowOnUppercasedDbName()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => new Settings(new Uri("http://example.com"), "UpprecasedName"));
		}

		[Fact]
		public void ShouldNotThrowOnIncorrectCharDbName()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() => new Settings(new Uri("http://example.com"), "name_with*in_the_middle"));
		}

		[Fact]
		public void ShouldNotThrowOnValidDbName()
		{
			Assert.DoesNotThrow(() => new Settings(new Uri("http://example.com"), "a0-9a-z_$()+-/"));
		}

		[Fact]
		public void ShouldReportIfIncomplete()
		{
			var settings = new Settings();
			Assert.True(settings.Incomplete);
			settings.ServerUri = new Uri("http://example.com");
			Assert.True(settings.Incomplete);
			settings.DefaultDatabaseName = "db1";
			Assert.False(settings.Incomplete);
		}
	}
}
