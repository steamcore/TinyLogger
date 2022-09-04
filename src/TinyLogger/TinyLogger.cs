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

		void GetMessageTokens(IList<MessageToken> output)
		{
			// Tokenize actual log message using a callback in case it is not needed (depends on the log template)
			var getMessage = (IList<MessageToken> output) => { messageTokenizer.Tokenize(state, exception, formatter, output); };

			using var data = Pooling.RentMetadataDictionary();

			// Setup built in log fields
			data.Value.Add("categoryName", MessageToken.FromLiteral(categoryName));
			data.Value.Add("eventId", eventId);
			data.Value.Add("exception", exception);
			data.Value.Add("exception_message", exception != null ? MessageToken.FromObject(exception, valueTransform: x => ((Exception)x).Message) : null);
			data.Value.Add("exception_newLine", exception != null ? newLine : null);
			data.Value.Add("logLevel", logLevel);
			data.Value.Add("message", getMessage);
			data.Value.Add("newLine", newLine);
			data.Value.Add("timestamp", MessageToken.FromLiteral(DateTime.Now));
			data.Value.Add("timestamp_utc", MessageToken.FromLiteral(DateTime.UtcNow));

			// Extend log fields
			for (var i = 0; i < extenders.Count; i++)
			{
				extenders[i].Extend(data.Value);
			}

			// Do the work
			messageTokenizer.Tokenize(data.Value, output);
		}
	}
}
