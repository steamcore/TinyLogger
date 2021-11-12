namespace TinyLogger.Extenders;

public class ThreadExtender : ILogExtender
{
	public void Extend(Dictionary<string, object?> data)
	{
		data["threadId"] = (Func<object>)(() => MessageToken.FromLiteral(Environment.CurrentManagedThreadId));
	}
}
