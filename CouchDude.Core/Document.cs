using System.Dynamic;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace CouchDude.Core
{
	/// <summary>Describes CouchDB document.</summary>
	public class Document : IDynamicMetaObjectProvider
	{
		private JObject jsonObject;

		/// <constructor />
		public Document() : this(new JObject()) { }

		/// <constructor />
		public Document(JObject jsonObject)
		{
			this.jsonObject = jsonObject;
		}

		/// <summary></summary>
		public string Id { get; set; }

		/// <summary></summary>
		public string Revision { get; set; }

		/// <summary></summary>
		public string Type { get; set; }
		
		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			IDynamicMetaObjectProvider jo = jsonObject;
			return jo.GetMetaObject(parameter);
		}
	}
}