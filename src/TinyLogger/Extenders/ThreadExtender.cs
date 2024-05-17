using System.Globalization;

namespace TinyLogger.Extenders;

public class ThreadExtender : ILogExtender
{
	public void Extend(Dictionary<string, MessageToken?> data)
	{
		if (data?.ContainsKey("threadId") == false)
		{
			data.Add("threadId", new FuncToken(() => new LiteralToken(Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture))));
		}
	}
}
