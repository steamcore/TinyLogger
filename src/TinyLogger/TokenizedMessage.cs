using Microsoft.Extensions.Logging;

namespace TinyLogger;

public abstract class TokenizedMessage(string categoryName, LogLevel logLevel)
{
	public string CategoryName { get; } = categoryName;
	public LogLevel LogLevel { get; } = logLevel;
	public abstract IReadOnlyList<MessageToken> MessageTokens { get; }
}

public sealed class PooledTokenizedMessage : TokenizedMessage, IDisposable
{
	private readonly PooledValue<List<MessageToken>> messageTokens;
	private readonly PooledValue<List<MessageToken>> logMessageTokens;

	public override IReadOnlyList<MessageToken> MessageTokens => logMessageTokens.Value;

	public PooledTokenizedMessage(string categoryName, LogLevel logLevel, Action<IList<MessageToken>> populateMessage, Action<IReadOnlyList<MessageToken>, IList<MessageToken>> populateLogMessage)
		: base(categoryName, logLevel)
	{
		ArgumentNullException.ThrowIfNull(populateMessage);
		ArgumentNullException.ThrowIfNull(populateLogMessage);

		messageTokens = Pooling.RentMessageTokenList();
		logMessageTokens = Pooling.RentMessageTokenList();

		populateMessage(messageTokens.Value);
		populateLogMessage(messageTokens.Value, logMessageTokens.Value);
	}

	public void Dispose()
	{
		messageTokens.Dispose();
		logMessageTokens.Dispose();
	}
}
