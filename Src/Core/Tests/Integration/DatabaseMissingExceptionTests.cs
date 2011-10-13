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
using CouchDude.Tests.SampleData;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class DatabaseMissingExceptionTests
	{
		[Fact]
		public void ShouldThrowDatabaseMissingExceptionIfNoDatabaseDefined()
		{
			var sessionFactory = Default.Settings.CreateSessionFactory();

			const string randomDbName = "a3438f034223481d8a7673f578b63098";
			
			using (var session = sessionFactory.CreateSession(databaseName: randomDbName))
				Assert.Throws<DatabaseMissingException>(() => session.Synchronously.Load<Entity>("doc1"));

			using (var session = sessionFactory.CreateSession(databaseName: randomDbName))
				Assert.Throws<DatabaseMissingException>(() => session.Synchronously.Query<ViewData>(new ViewQuery { ViewName = "_all_docs" }));

			using (var session = sessionFactory.CreateSession(databaseName: randomDbName))
				Assert.Throws<DatabaseMissingException>(
					() => session.Synchronously.QueryLucene<ViewData>(new LuceneQuery { DesignDocumentName = "dd", IndexName = "ld" }));
		}
	}
}