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
		{
			throw new ArgumentNullException(nameof(options));
		}
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
		{
			throw new ArgumentNullException(nameof(formatter));
		}

		if (output is null)
		{
			throw new ArgumentNullException(nameof(output));
		}
#endif

		using var data = Pooling.RentMetadataDictionary();

		PopulateDictionary(state, data.Value);

		if (data.Value.Count > 1 && data.Value.TryGetValue("{OriginalFormat}", out var token) && token?.TryGetValue<string>(out var originalFormat) == true)
		{
			var template = CachedTemplateTokenizer.Tokenize(originalFormat);

			Tokenize(template, data.Value, output);

			return;
		}

		output.Add(new LiteralToken(formatter(state, exception)));

		static void PopulateDictionary(TState state, Dictionary<string, MessageToken?> dictionary)
		{
			if (state is IReadOnlyList<KeyValuePair<string, object?>> logValues)
			{
				for (var i = 0; i < logValues.Count; i++)
				{
					var kvp = logValues[i];

					dictionary[kvp.Key] = new ObjectToken<object>(kvp.Value);
				}
			}
		}
	}

	public void Tokenize(IReadOnlyDictionary<string, MessageToken?> data, IList<MessageToken> output)
	{
		Tokenize(tokenizedMessageTemplate, data, output);
	}

	public void Tokenize(IReadOnlyList<MessageToken> messageTokens, IReadOnlyDictionary<string, MessageToken?> data, IList<MessageToken> output)
	{
#if NET
		ArgumentNullException.ThrowIfNull(messageTokens);
		ArgumentNullException.ThrowIfNull(data);
		ArgumentNullException.ThrowIfNull(output);
#else
		if (messageTokens is null)
		{
			throw new ArgumentNullException(nameof(messageTokens));
		}

		if (data is null)
		{
			throw new ArgumentNullException(nameof(data));
		}

		if (output is null)
		{
			throw new ArgumentNullException(nameof(output));
		}
#endif

		for (var i = 0; i < messageTokens.Count; i++)
		{
			var messageToken = messageTokens[i];

			if (messageToken is LiteralToken)
			{
				output.Add(messageToken);
			}
			else if (messageToken.TryGetValue<string>(out var key) && data.ContainsKey(key))
			{
				AddTokens(objectTokenizer, data[key]?.WithFormatFrom(messageToken as ObjectToken), output);
			}
		}
	}

	private static void AddTokens(IObjectTokenizer? objectTokenizer, MessageToken? token, IList<MessageToken> output)
	{
		switch (token)
		{
			case FuncToken lazyToken:
				AddTokens(objectTokenizer, lazyToken.GetToken(), output);
				break;

			case TokenTemplate tokenTemplate:
				{
					foreach (var item in tokenTemplate.MessageTokens)
					{
						AddTokens(objectTokenizer, item.WithFormatFrom(token as ObjectToken), output);
					}
				}
				break;

			case LiteralToken literalToken:
				output.Add(literalToken);
				break;

			case ObjectToken valueToken:
				if (!valueToken.TryGetValue(out var value) || objectTokenizer?.TryToTokenize(value, output) != true)
				{
					output.Add(valueToken.WithFormatFrom(token as ObjectToken));
				}
				break;
		}
	}
}
