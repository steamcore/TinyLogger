using System.Globalization;
using Microsoft.Extensions.Logging;
using TinyLogger.Tokenizers;

namespace TinyLogger;

internal class TinyLogger(
	IMessageTokenizer messageTokenizer,
	IReadOnlyList<ILogExtender> extenders,
	ILogRenderer renderer,
	string categoryName
)
	: ILogger
{
	private static readonly MessageToken newLine = new LiteralToken(Environment.NewLine);

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		return NullDisposable.Instance;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return logLevel != LogLevel.None;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		renderer.Render(new TokenizedMessage(categoryName, logLevel, GetMessageTokens)).ConfigureAwait(false).GetAwaiter().GetResult();

		void GetMessageTokens(IList<MessageToken> output)
		{
			using var data = Pooling.RentMetadataDictionary();

			// Setup built in log fields
			data.Value.Add("categoryName", new LiteralToken(categoryName));
			data.Value.Add("eventId", new ObjectToken<EventId>(eventId));
			data.Value.Add("exception", new ObjectToken<Exception>(exception));
			data.Value.Add("exception_message", exception != null ? new ObjectTokenWithTransform<Exception>(exception, static x => x.Message) : null);
			data.Value.Add("exception_newLine", exception != null ? newLine : null);
			data.Value.Add("logLevel", new ObjectToken<LogLevel>(logLevel));
			data.Value.Add("message", new TokenTemplate(tokens => messageTokenizer.Tokenize(state, exception, formatter, tokens)));
			data.Value.Add("newLine", newLine);
			data.Value.Add("timestamp", new LiteralToken(DateTime.Now.ToString(CultureInfo.CurrentCulture)));
			data.Value.Add("timestamp_utc", new LiteralToken(DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)));

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
