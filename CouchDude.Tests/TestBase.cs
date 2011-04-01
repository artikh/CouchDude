using System.Collections.Specialized;

namespace CouchDude.Tests
{
	public abstract class TestBase
	{
		static TestBase()
		{
			var properties = new NameValueCollection();
			properties["showDateTime"] = "true";
			properties["showLogName"] = "true";
			properties["level"] = "DEBUG";
			properties["dateTimeFormat"] = "yyyy-MM-dd HH:mm:ss:fff";

			Common.Logging.LogManager.Adapter = 
				new Common.Logging.Simple.TraceLoggerFactoryAdapter(properties);      
		}
	}
}
