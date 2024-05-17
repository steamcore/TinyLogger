using Microsoft.Extensions.Logging;

namespace TinyLogger;

public class TokenizedMessage(string categoryName, LogLevel logLevel, Action<IList<MessageToken>> createMessageTokens)
{
	public string CategoryName { get; } = categoryName;
	public LogLevel LogLevel { get; } = logLevel;

	public PooledValue<List<MessageToken>> RentMessageTokenList()
	{
		var messageTokens = Pooling.RentMessageTokenList();
		createMessageTokens(messageTokens.Value);
		return messageTokens;
	}
}
