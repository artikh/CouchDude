using System;
using System.Runtime.Serialization;

namespace CouchDude.Utils
{
	/// <summary><see cref="Exception"/>-related utilities.</summary>
	public static class ExceptionUtils
	{
		/// <summary>Makes sure exception stack trace would not be modify on rethrow.</summary>
		public static Exception PreserveStackTrace(this Exception exception)
		{
			var streamingContext = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var objectManager = new ObjectManager(null, streamingContext);
			var serializationInfo = new SerializationInfo(exception.GetType(), new FormatterConverter());

			exception.GetObjectData(serializationInfo, streamingContext);
			objectManager.RegisterObject(exception, 1, serializationInfo); // prepare for SetObjectData
			objectManager.DoFixups(); // ObjectManager calls SetObjectData
			return exception;
		}
	}
}
