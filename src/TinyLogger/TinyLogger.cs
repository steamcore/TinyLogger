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
	private static readonly Dictionary<LogLevel, ObjectToken<LogLevel>> logLevelTokens = new()
	{
		[LogLevel.Trace] = new(LogLevel.Trace),
		[LogLevel.Debug] = new(LogLevel.Debug),
		[LogLevel.Information] = new(LogLevel.Information),
		[LogLevel.Warning] = new(LogLevel.Warning),
		[LogLevel.Error] = new(LogLevel.Error),
		[LogLevel.Critical] = new(LogLevel.Critical),
		[LogLevel.None] = new(LogLevel.None)
	};

	private static readonly MessageToken newLine = new LiteralToken(Environment.NewLine);
	private static readonly MessageToken timestamp = new FuncToken(() => new LiteralToken(DateTime.Now.ToString(CultureInfo.CurrentCulture)));
	private static readonly MessageToken timestampUtc = new FuncToken(() => new LiteralToken(DateTime.UtcNow.ToString(CultureInfo.CurrentCulture)));

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
		renderer.RenderAsync(new TokenizedMessage(categoryName, logLevel, GetMessageTokens)).ConfigureAwait(false).GetAwaiter().GetResult();

		void GetMessage(IList<MessageToken> output)
		{
			messageTokenizer.Tokenize(state, exception, formatter, output);
		}

		void GetMessageTokens(IList<MessageToken> output)
		{
			using var data = Pooling.RentMetadataDictionary();

			// Setup built in log fields
			data.Value.Add("categoryName", new LiteralToken(categoryName));
			data.Value.Add("eventId", new ObjectToken<EventId>(eventId));
			data.Value.Add("logLevel", logLevelTokens[logLevel]);
			data.Value.Add("message", new TokenTemplate(GetMessage));
			data.Value.Add("newLine", newLine);
			data.Value.Add("timestamp", timestamp);
			data.Value.Add("timestamp_utc", timestampUtc);

			if (exception is not null)
			{
				data.Value.Add("exception", new ObjectToken<Exception>(exception));
				data.Value.Add("exception_message", new ObjectTokenWithTransform<Exception>(exception, GetExceptionMessage));
				data.Value.Add("exception_newLine", newLine);
			}

			// Extend log fields
			for (var i = 0; i < extenders.Count; i++)
			{
				extenders[i].Extend(data.Value);
			}

			// Do the work
			messageTokenizer.Tokenize(data.Value, output);
		}

		static string GetExceptionMessage(Exception exception)
		{
			return exception.Message;
		}
	}
}
