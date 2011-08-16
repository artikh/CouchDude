using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CouchDude.Core.Utils
{
	/// <summary><see cref="Task"/> utility methods.</summary>
	public static class TaskUtils
	{
		/// <summary>Waits for result of the task returning it result.</summary>
		public static void WaitForResult(this Task task)
		{
			try
			{
				task.Wait();
			}
			catch (AggregateException e)
			{
				if (e.InnerExceptions.Count == 1)
					throw PreserveStackTrace(e);
				throw;
			}
		}
		/// <summary>Waits for result of the task returning it result.</summary>
		public static T WaitForResult<T>(this Task<T> task)
		{
			try
			{
				task.Wait();
				return task.Result;
			}
			catch (AggregateException e)
			{
				if (e.InnerExceptions.Count == 1)
					throw PreserveStackTrace(e);
				throw;
			}
		}


		private static Exception PreserveStackTrace(Exception outerException)
		{
			var innerException = outerException.InnerException;

			var streamingContext = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var objectManager = new ObjectManager(null, streamingContext);
			var serializationInfo = new SerializationInfo(innerException.GetType(), new FormatterConverter());

			innerException.GetObjectData(serializationInfo, streamingContext);
			objectManager.RegisterObject(innerException, 1, serializationInfo); // prepare for SetObjectData
			objectManager.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString

			return innerException;
		}
	}
}