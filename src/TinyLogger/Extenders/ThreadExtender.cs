using System.Globalization;

namespace TinyLogger.Extenders;

public class ThreadExtender : ILogExtender
{
	private static readonly FuncToken threadId = new(() => new LiteralToken(Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture)));

	public void Extend(Dictionary<string, MessageToken?> data)
	{
		if (data?.ContainsKey("threadId") == false)
		{
			data.Add("threadId", threadId);
		}
	}
}
