using Microsoft.Extensions.Logging;

namespace TinyLogger;

public class TokenizedMessage
{
	private readonly Action<IList<MessageToken>> createMessageTokens;

	public string CategoryName { get; }
	public LogLevel LogLevel { get; }

	public TokenizedMessage(string categoryName, LogLevel logLevel, Action<IList<MessageToken>> createMessageTokens)
	{
		CategoryName = categoryName;
		LogLevel = logLevel;

		this.createMessageTokens = createMessageTokens;
	}

	public IPooledValue<List<MessageToken>> RentMessageTokenList()
	{
		var messageTokens = Pooling.RentMessageTokenList();
		createMessageTokens(messageTokens.Value);
		return messageTokens;
	}
}
