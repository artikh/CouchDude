#region Licence Info 
/*
	Copyright 2011 · Artem Tikhomirov																					
																																					
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

		/// <summary>Extracts single inner exception or flatterned <see cref="AggregateException"/>.</summary>
		public static Exception Extract(this AggregateException e)
		{
			var flattenedAggregateException = e.Flatten();
			if (flattenedAggregateException.InnerExceptions.Count == 1)
				return PreserveStackTrace(flattenedAggregateException.InnerException);
			
			return flattenedAggregateException;
		}
	}
}
