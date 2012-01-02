using System;
using System.Json;

namespace CouchDude.Utils
{
	/// <summary>Extends <see cref="System.Json.JsonValue"/> classes to accept instance of <see cref="IJsonVisitor"/></summary>
	public static class JsonVisitorExtensions
	{
		/// <summary>Accepts visitor for <see cref="JsonArray"/> instance.</summary>
		public static void AcceptVisitor(this JsonArray self, IJsonVisitor visitor) { visitor.Visit(self); }

		/// <summary>Accepts visitor for <see cref="JsonObject"/> instance.</summary>
		public static void AcceptVisitor(this JsonObject self, IJsonVisitor visitor) { visitor.Visit(self); }

		/// <summary>Accepts visitor for <see cref="JsonPrimitive"/> instance.</summary>
		public static void AcceptVisitor(this JsonPrimitive self, IJsonVisitor visitor)
		{
			switch (self.JsonType)
			{
				case JsonType.String:
					visitor.Visit((string)self.Value);
					break;
				case JsonType.Number:
					if(self.Value is double || self.Value is float)
						visitor.Visit((double)self.Value);
					else
						visitor.Visit((int)self.Value);
					break;
				case JsonType.Boolean:
					visitor.Visit((bool)self.Value);
					break;
				case JsonType.Default:
					visitor.VisitDefault();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	/// <summary>Interface class for <see cref="System.Json.JsonValue"/> algorithms.</summary>
	public interface IJsonVisitor
	{
		/// <summary>Performs visit of an array.</summary>
		void Visit(JsonArray array);
		
		/// <summary>Performs visit of a string.</summary>
		void Visit(string str);
		
		/// <summary>Performs visit of a floating point number.</summary>
		void Visit(double number);
		
		/// <summary>Performs visit of a integer number.</summary>
		void Visit(int number);
		
		/// <summary>Performs visit of an boolean value.</summary>
		void Visit(bool flag);

		/// <summary>Performs visit of default value.</summary>
		void VisitDefault();

		/// <summary>Performs visit of an object.</summary>
		void Visit(JsonObject obj);
	}
}
