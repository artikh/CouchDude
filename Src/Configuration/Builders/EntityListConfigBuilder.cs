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

using System.Reflection;

namespace CouchDude.Configuration.Builders
{
	/// <summary>Helps <see cref="SettingsBuilder"/> to build list of entity config class.</summary>
	public class EntityListConfigBuilder : EntityListConfigBuilder<EntityListConfigBuilder>
	{
		/// <constructor />
		public EntityListConfigBuilder(SettingsBuilder parent) : base(parent) {}
	}

	/// <summary>Helps <see cref="SettingsBuilder"/> to build list of entity config class.</summary>
	public class EntityListConfigBuilder<TSelf> : EntityConfigBuilder<TSelf> where TSelf: EntityListConfigBuilder<TSelf>
	{
		/// <constructor />
		public EntityListConfigBuilder(SettingsBuilder parent): base(parent) { }
			
		/// <summary>Adds assembly where provided type is declared to entity classes scan list.</summary>
		public TSelf FromAssemblyOf<T>()
		{
			AssembliesToScan.Add(typeof (T).Assembly);
			return (TSelf)this;
		}

		/// <summary>Adds assembly of provided name to entity classes scan list.</summary>
		public TSelf FromAssembly(string assemblyNameString)
		{
			if(String.IsNullOrEmpty(assemblyNameString)) throw new ArgumentNullException("assemblyNameString");
			

			AssembliesToScan.Add(Assembly.Load(assemblyNameString));
			return (TSelf)this;
		}

		/// <summary>Adds base class restriction for found entity types.</summary>
		public TSelf InheritedFrom<T>()
		{
			if (typeof(T).IsInterface)
				throw new ArgumentException(
					String.Format("Type {0} is an interface. Probably you should use Implements<T>() method insted.", typeof(T)));
			

			Predicates.Add(type => typeof(T).IsAssignableFrom(type));
			return (TSelf)this;
		}

		/// <summary>Restricts entities to ones inherited from <typeparamref name="T"/>.</summary>
		public TSelf Implementing<T>()
		{
			if (!typeof(T).IsInterface)
				throw new ArgumentException(
					String.Format("Type {0} is not an interface. Probably you should use InheritedFrom<T>() method insted.", typeof(T)));
			

			Predicates.Add(type => typeof(T).IsAssignableFrom(type));
			return (TSelf) this;
		}

		/// <summary>Adds custom entity type search predicate.</summary>
		public TSelf Where(Predicate<Type> predicate)
		{
			if(predicate == null) throw new ArgumentNullException("predicate");
			

			Predicates.Add(predicate);
			return (TSelf) this;
		}
	}
}