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
using System.Threading.Tasks;

namespace CouchDude.Utils
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
				throw e.Extract();
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
				throw e.Extract();
			}
		}
	}
}