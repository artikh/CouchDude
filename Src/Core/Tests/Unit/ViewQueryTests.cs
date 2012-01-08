﻿#region Licence Info 
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

namespace CouchDude.Tests.Unit
{
	public class ViewQueryTests
	{
		[Fact]
		public void ShouldValidateNegativeNumbers() 
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new ViewQuery().Limit = -1);
			Assert.Throws<ArgumentOutOfRangeException>(() => new ViewQuery().Skip = -1);
			Assert.Throws<ArgumentOutOfRangeException>(() => new ViewQuery().GroupLevel = -1);
		}

		[Fact]
		public void ShouldConvertFromStringAndBackUsingParseAndToString()
		{
			const string testUri =
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&descending=true&include_docs=true&inclusive_end=false&group=true&group_level=42&stale=update_after";

			var viewQuery = ViewQuery.Parse(testUri);
			var generatedUri = viewQuery.ToString();

			Assert.Equal(testUri, generatedUri);
		}

		[Fact]
		public void ShouldConvertFromUriAndBackUsingParseAndToUri()
		{
			var testUri = new Uri(
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
					"&limit=42&skip=42&descending=true&include_docs=true&inclusive_end=false&group=true&group_level=42&stale=update_after", 
				UriKind.Relative);

			var viewQuery = ViewQuery.Parse(testUri);
			var generatedUri = viewQuery.ToUri();

			Assert.Equal(testUri, generatedUri);
		}

		[Fact]
		public void ShouldClone() 
		{
			var query = ViewQuery.Parse(
				"_design/dd/_view/pointOfView?startkey=%22first+key%22&startkey_docid=start+dockey&endkey=%22second+key%22&endkey_docid=end+dockey" +
				"&limit=42&skip=42&stale=update_after&descending=true&include_docs=true&inclusive_end=false&group=true&group_level=42");

			var clone = query.Clone();

			Assert.Equal(query, clone);
		}

		[Fact]
		public void ShouldResetGroupingIfSuppressReduce()
		{
			var query = new ViewQuery {
				Group = true,
				GroupLevel = 42
			};

			query.SuppressReduce = true;

			Assert.False(query.Group);
			Assert.Null(query.GroupLevel);
			Assert.Throws<InvalidOperationException>(() => query.Group = true);
			Assert.Throws<InvalidOperationException>(() => query.GroupLevel = 42);
		}

		[Fact]
		public void ShouldEnforceGroupingIfGroupLevelSet()
		{
			var query = new ViewQuery {
				Group = false
			};

			query.GroupLevel = 42;

			Assert.True(query.Group);
			Assert.Throws<InvalidOperationException>(() => query.Group = false);
		}
	}
}
