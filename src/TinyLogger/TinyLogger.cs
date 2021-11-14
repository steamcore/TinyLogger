using Microsoft.Extensions.Logging;
using TinyLogger.Tokenizers;

namespace TinyLogger;

internal class TinyLogger : ILogger
{
	private static readonly MessageToken newLine = MessageToken.FromLiteral(Environment.NewLine);

	private readonly IMessageTokenizer messageTokenizer;
	private readonly IReadOnlyList<ILogExtender> extenders;
	private readonly ILogRenderer renderer;
	private readonly string categoryName;

	public TinyLogger(IMessageTokenizer messageTokenizer, IReadOnlyList<ILogExtender> extenders, ILogRenderer renderer, string categoryName)
	{
		this.messageTokenizer = messageTokenizer;
		this.extenders = extenders;
		this.renderer = renderer;
		this.categoryName = categoryName;
	}

	public IDisposable BeginScope<TState>(TState state)
	{
		return NullDisposable.Instance;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return logLevel != LogLevel.None;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		renderer.Render(new TokenizedMessage(categoryName, logLevel, GetMessageTokens)).GetAwaiter().GetResult();

		IReadOnlyList<MessageToken> GetMessageTokens()
		{
			// Tokenize actual log message using a callback in case it is not needed (depends on the log template)
			var getMessage = () => messageTokenizer.Tokenize(state, exception, formatter);

			// Setup built in log fields
			var data = new Dictionary<string, object?>
			{
				{ "categoryName", MessageToken.FromLiteral(categoryName) },
				{ "eventId", eventId },
				{ "exception", exception },
				{ "exception_message", exception != null ? MessageToken.FromObject(exception, valueTransform: x => ((Exception)x).Message) : null },
				{ "exception_newLine", exception != null ? newLine : null },
				{ "logLevel", logLevel },
				{ "message", getMessage },
				{ "newLine", newLine },
				{ "timestamp", MessageToken.FromLiteral(DateTime.Now) },
				{ "timestamp_utc", MessageToken.FromLiteral(DateTime.UtcNow) }
			};

			// Extend log fields
			foreach (var extender in extenders)
			{
				extender.Extend(data);
			}

			// Do the work
			return messageTokenizer.Tokenize(data);
		}
	}
}
