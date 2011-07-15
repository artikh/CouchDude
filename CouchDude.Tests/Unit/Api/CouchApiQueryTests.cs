using System;
using System.Net.Http;
using CouchDude.Core;
using CouchDude.Core.Api;
using CouchDude.Core.Impl;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CouchDude.Tests.Unit.Api
{
	public class CouchApiQueryTests
	{
		[Fact]
		public void ShouldMapTipicalResponseToGetAllQuery()
		{
			var httpClientMock = new HttpClientMock(new {
				total_rows = 2,
				offset = 1,
				rows = new [] {
					new {
						key = new object[] { "key", 0 },
						value = "some string",
						id = "doc1",
						doc = new { _id = "doc1" }
					}	
				}
			}.ToJson());

			var couchApi = new CouchApi(httpClientMock, new Uri("http://example.com:5984/"), "testdb");
			var result = couchApi.Query(new ViewQuery {
				ViewName = "_all_docs",
				Key = new object[] { "key", 0 },
				Skip = 1,
				IncludeDocs = true
			});

			Assert.NotNull(result);
			Assert.Equal(2, result.TotalRows);
			Assert.Equal(1, result.Rows.Count);
			Assert.Equal(new object[] { "key", 0 }.ToJToken(),	result.Rows[0].Key,					new JTokenEqualityComparer());
			Assert.Equal(JValue.CreateString("some string"),		result.Rows[0].Value,				new JTokenEqualityComparer());
			Assert.Equal("doc1",																result.Rows[0].DocumentId);
			Assert.Equal(new { _id = "doc1" }.ToJObject(),			result.Rows[0].Document,		new JTokenEqualityComparer());
		}

		[Fact]
		public void ShouldSendGetRequestForGetAllQuery()
		{
			var httpClientMock = new HttpClientMock(new {
				total_rows = 2,
				offset = 1,
				rows = new object [0]	
				}
			.ToJson());

			var couchApi = new CouchApi(httpClientMock, new Uri("http://example.com:5984/"), "testdb");
			couchApi.Query(new ViewQuery {
				DesignDocumentName = "dd",
				ViewName = "v1",
				Skip = 1,
				IncludeDocs = true
			});


			Assert.Equal(HttpMethod.Get, httpClientMock.Request.Method);
			Assert.Equal(
				"http://example.com:5984/testdb/_design/dd/_view/v1?skip=1&include_docs=true", 
				httpClientMock.Request.RequestUri.ToString());
		}

		[Fact]
		public void ShouldThrowOnNullQuery()
		{
			var couchApi = new CouchApi(new HttpClientMock(), new Uri("http://example.com:5984/"), "testdb");
			Assert.Throws<ArgumentNullException>(() => couchApi.Query(null));
		}

		[Fact]
		public void ShouldThrowOnSkipMoreThen9()
		{
			var couchApi = new CouchApi(new HttpClientMock(), new Uri("http://example.com:5984/"), "testdb");
			Assert.Throws<ArgumentException>(() => couchApi.Query(new ViewQuery{ Skip = 10 }));
		}
	}
}