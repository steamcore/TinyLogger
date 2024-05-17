using System.Globalization;

namespace TinyLogger.Extenders;

public class ThreadExtender : ILogExtender
{
	public void Extend(Dictionary<string, object?> data)
	{
		if (data?.ContainsKey("threadId") == false)
		{
			data.Add("threadId", (Func<object>)(() => new LiteralToken(Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture))));
		}
	}
}
