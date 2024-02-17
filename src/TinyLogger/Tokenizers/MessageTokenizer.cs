using Microsoft.Extensions.Options;

namespace TinyLogger.Tokenizers;

public class MessageTokenizer : IMessageTokenizer
{
	private readonly IObjectTokenizer? objectTokenizer;
	private readonly Lazy<IReadOnlyList<MessageToken>> tokenizedMessageTemplate;

	public MessageTokenizer(IOptions<TinyLoggerOptions> options)
	{
#if NET
		ArgumentNullException.ThrowIfNull(options);
#else
		if (options is null)
			throw new ArgumentNullException(nameof(options));
#endif

		objectTokenizer = options.Value.ObjectTokenizer;

		tokenizedMessageTemplate = new Lazy<IReadOnlyList<MessageToken>>(() => [.. TemplateTokenizer.Tokenize(options.Value.Template)]);
	}

	public void Tokenize<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter, IList<MessageToken> output)
	{
#if NET
		ArgumentNullException.ThrowIfNull(formatter);
		ArgumentNullException.ThrowIfNull(output);
#else
		if (formatter is null)
			throw new ArgumentNullException(nameof(formatter));

		if (output is null)
			throw new ArgumentNullException(nameof(output));
#endif

		using var data = Pooling.RentMetadataDictionary();

		PopulateDictionary(state, data.Value);

		if (data.Value.Count > 1 && data.Value.TryGetValue("{OriginalFormat}", out var value) && value is string originalFormat)
		{
			using var template = Pooling.RentMessageTokenList();

			TemplateTokenizer.Tokenize(originalFormat, template.Value);

			Tokenize(template.Value, data.Value, output);

			return;
		}

		output.Add(MessageToken.FromLiteral(formatter(state, exception)));

		static void PopulateDictionary(TState state, Dictionary<string, object?> dictionary)
		{
			if (state is IReadOnlyList<KeyValuePair<string, object?>> logValues)
			{
				for (var i = 0; i < logValues.Count; i++)
				{
					var kvp = logValues[i];

					dictionary[kvp.Key] = kvp.Value;
				}
			}
		}
	}

	public void Tokenize(IReadOnlyDictionary<string, object?> data, IList<MessageToken> output)
	{
		Tokenize(tokenizedMessageTemplate.Value, data, output);
	}

	public void Tokenize(IReadOnlyList<MessageToken> messageTokens, IReadOnlyDictionary<string, object?> data, IList<MessageToken> output)
	{
#if NET
		ArgumentNullException.ThrowIfNull(messageTokens);
		ArgumentNullException.ThrowIfNull(data);
		ArgumentNullException.ThrowIfNull(output);
#else
		if (messageTokens is null)
			throw new ArgumentNullException(nameof(messageTokens));

		if (data is null)
			throw new ArgumentNullException(nameof(data));

		if (output is null)
			throw new ArgumentNullException(nameof(output));
#endif

		for (var i = 0; i < messageTokens.Count; i++)
		{
			var messageToken = messageTokens[i];

			if (messageToken.Type == MessageTokenType.LiteralToken)
			{
				output.Add(messageToken);
			}
			else if (messageToken.Value is string key && data.ContainsKey(key))
			{
				AddTokens(objectTokenizer, messageToken, data[key], output);
			}
		}
	}

	private static void AddTokens(IObjectTokenizer? objectTokenizer, MessageToken messageToken, object? value, IList<MessageToken> output)
	{
		if (value is null)
		{
			if (messageToken.Value is "null")
			{
				output.Add(MessageToken.FromLiteral("(null)", messageToken.Alignment, messageToken.Format));
			}

			return;
		}

		if (value is Action<IList<MessageToken>> valueAction)
		{
			valueAction(output);

			return;
		}

		if (value is Func<object> valueFunc)
		{
			value = valueFunc();
		}

		var hasFormat = messageToken.Alignment != null || messageToken.Format != null;

		if (value is IReadOnlyList<MessageToken> valueTokens)
		{
			for (var i = 0; i < valueTokens.Count; i++)
			{
				var token = valueTokens[i];

				output.Add(hasFormat ? token.WithFormat(messageToken) : token);
			}
			return;
		}

		if (value is MessageToken valueToken)
		{
			output.Add(hasFormat ? valueToken.WithFormat(messageToken) : valueToken);
			return;
		}

		if (objectTokenizer?.TryToTokenize(value, output) != true)
		{
			output.Add(MessageToken.FromObject(value, messageToken.Alignment, messageToken.Format));
		}
	}
}
