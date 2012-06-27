using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CouchDude.Api
{
	static class CouchDbRequester
	{
		public static async Task<HttpResponseMessage> RequestCouchDb(this CouchApi self, HttpRequestMessage request)
		{
			try
			{
				return await self.SendAsync(request);
			}
			catch(Exception e)
			{
				var aggregateException = e as AggregateException;
				if (aggregateException != null)
				{
					var innerExceptions = aggregateException.InnerExceptions;

					var newInnerExceptions = new Exception[innerExceptions.Count];
					for (var i = 0; i < innerExceptions.Count; i++)
						newInnerExceptions[i] = WrapIfNeeded(innerExceptions[i]);
					throw new AggregateException(aggregateException.Message, newInnerExceptions);
				}
				
				throw WrapIfNeeded(e);
			}
		}

		private static Exception WrapIfNeeded(Exception e)
		{
			return e is WebException || e is SocketException || e is HttpRequestException
				? new CouchCommunicationException(e) : e;
		}
	}
}