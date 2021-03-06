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
using System.Net.Http;
using CouchDude.Api;
using Xunit;

namespace CouchDude.Tests.Integration
{
	[IntegrationTest]
	public class CouchApiBulkUpdate
	{
		[Fact]
		public void ShouldDoUpdateInBulkFlowlessly()
		{
			var couchApi = (ICouchApi) new CouchApi(new CouchApiSettings("http://127.0.0.1:5984/"), null);
			var dbApi = couchApi.Db("testdb");
			dbApi.Synchronously.Create(throwIfExists: false);

			var doc1Id = Guid.NewGuid() + ".doc1";
			var doc2Id = Guid.NewGuid() + ".doc2";
			var doc3Id = Guid.NewGuid() + ".doc3";

			var doc2Result = dbApi.Synchronously.SaveDocument(new {_id = doc2Id, name = "Стас Гиркин"}.ToDocument());
			var doc3Result = dbApi.Synchronously.SaveDocument(new {_id = doc3Id, name = "John Dow"}.ToDocument());

			var result = dbApi.Synchronously.BulkUpdate(
				x => {
					x.Create(new {_id = doc1Id, name = "James Scully"}.ToDocument());
					x.Update(new {_id = doc2Id, _rev = doc2Result.Revision, name = "Стас Гиркин", age = 42}.ToDocument());
					x.Delete(doc3Result.Id, doc3Result.Revision);
				});

			Assert.Equal(3, result.Count);
			Assert.Equal(doc1Id, result[doc1Id].Id);
			Assert.Equal(doc2Id, result[doc2Id].Id);
			Assert.Equal(doc3Id, result[doc3Id].Id);

			var loadedDoc1 = dbApi.Synchronously.RequestDocument(doc1Id);
			Assert.NotNull(loadedDoc1);
			Assert.Equal("James Scully", (string)loadedDoc1.RawJsonObject["name"]);

			var loadedDoc2 = dbApi.Synchronously.RequestDocument(doc2Id);
			Assert.NotNull(loadedDoc2);
			Assert.Equal("Стас Гиркин", (string)loadedDoc2.RawJsonObject["name"]);
			Assert.Equal(42, (int)loadedDoc2.RawJsonObject["age"]);

			var loadedDoc3 = dbApi.Synchronously.RequestDocument(doc3Id);
			Assert.Null(loadedDoc3);

			dbApi.DeleteDocument(doc1Id, (string)loadedDoc1.RawJsonObject["_rev"]);
			dbApi.DeleteDocument(doc2Id, (string)loadedDoc2.RawJsonObject["_rev"]);
		}
	}
}
