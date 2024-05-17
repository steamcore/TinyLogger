using Microsoft.Extensions.Options;

namespace TinyLogger.Tokenizers;

public class MessageTokenizer : IMessageTokenizer
{
	private readonly IObjectTokenizer? objectTokenizer;
	private readonly IReadOnlyList<MessageToken> tokenizedMessageTemplate;

	public MessageTokenizer(IOptions<TinyLoggerOptions> options)
	{
#if NET
		ArgumentNullException.ThrowIfNull(options);
#else
		if (options is null)
			throw new ArgumentNullException(nameof(options));
#endif

		objectTokenizer = options.Value.ObjectTokenizer;
		tokenizedMessageTemplate = CachedTemplateTokenizer.Tokenize(options.Value.Template);
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
			var template = CachedTemplateTokenizer.Tokenize(originalFormat);

			Tokenize(template, data.Value, output);

			return;
		}

		output.Add(new LiteralToken(formatter(state, exception)));

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
		Tokenize(tokenizedMessageTemplate, data, output);
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

			if (messageToken is LiteralToken)
			{
				output.Add(messageToken);
			}
			else if (messageToken is ObjectToken objectToken && objectToken.Value is string key && data.ContainsKey(key))
			{
				AddTokens(objectTokenizer, objectToken, data[key], output);
			}
		}
	}

	private static void AddTokens(IObjectTokenizer? objectTokenizer, ObjectToken objectToken, object? value, IList<MessageToken> output)
	{
		if (value is null)
		{
			if (objectToken.Value is "null")
			{
				output.Add(new LiteralToken("(null)"));
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

		var hasFormat = objectToken.Alignment != null || objectToken.Format != null;

		if (value is IReadOnlyList<MessageToken> valueTokens)
		{
			for (var i = 0; i < valueTokens.Count; i++)
			{
				var token = valueTokens[i];

				output.Add(hasFormat && token is ObjectToken ot ? ot with { Alignment = objectToken.Alignment, Format = objectToken.Format } : token);
			}
			return;
		}

		if (value is LiteralToken literalToken)
		{
			output.Add(literalToken);
			return;
		}

		if (value is ObjectToken valueToken)
		{
			output.Add(hasFormat ? valueToken with { Alignment = objectToken.Alignment, Format = objectToken.Format } : valueToken);
			return;
		}

		if (objectTokenizer?.TryToTokenize(value, output) != true)
		{
			output.Add(new ObjectToken(value, objectToken.Alignment, objectToken.Format));
		}
	}
}
