#region Licence Info 
/*
	Copyright 2011 � Artem Tikhomirov																					
																																					
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

using CouchDude.Api;
using CouchDude.Impl;
using CouchDude.Tests.SampleData;
using Moq;
using Xunit;

namespace CouchDude.Tests.Unit.Core.Impl
{
	public class CouchSessionConstuctorTests
	{
		[Fact]
		public void ShouldThrowOnNullParameters()
		{
			Assert.Throws<ArgumentNullException>(() => new CouchSession(Default.Settings, null));
			Assert.Throws<ArgumentNullException>(() => new CouchSession(null, Mock.Of<ICouchApi>()));
		}

		[Fact]
		public void ShouldThrowOnUnfinishedSettings()
		{
			Assert.Throws<ArgumentException>(() => 
				new CouchSession(new Settings(), Mock.Of<ICouchApi>())
			);
		}
	}
}