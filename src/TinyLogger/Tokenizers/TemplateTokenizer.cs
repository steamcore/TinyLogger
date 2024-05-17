namespace TinyLogger.Tokenizers;

public static class CachedTemplateTokenizer
{
	private static readonly LimitedSizeCache<string, List<MessageToken>> cache = new(1_000);

	public static IReadOnlyList<MessageToken> Tokenize(string logFormat)
	{
		if (cache.TryGetValue(logFormat, out var tokens))
		{
			return tokens!;
		}

		var result = new List<MessageToken>();
		TemplateTokenizer.Tokenize(logFormat, result);
		cache.Put(logFormat, result);
		return result;
	}
}

public static class TemplateTokenizer
{
	private enum State
	{
		Literal,
		OpenBrace,
		Value,
		CloseBrace
	}

	public static IReadOnlyList<MessageToken> Tokenize(string logFormat)
	{
		var result = new List<MessageToken>();
		Tokenize(logFormat, result);
		return result;
	}

	public static void Tokenize(string logFormat, IList<MessageToken> output)
	{
#if NET
		ArgumentNullException.ThrowIfNull(logFormat);
		ArgumentNullException.ThrowIfNull(output);
#else
		if (logFormat is null)
			throw new ArgumentNullException(nameof(logFormat));

		if (output is null)
			throw new ArgumentNullException(nameof(output));
#endif

		var state = State.Literal;
		var start = 0;
		var lastOpen = -1;
		var lastClose = -1;

		for (var i = 0; i < logFormat.Length; i++)
		{
			var c = logFormat[i];

			switch (state)
			{
				case State.Literal when c == '{':
					state = State.OpenBrace;
					lastOpen = i;
					break;

				case State.OpenBrace when c == '{':
					state = State.Literal;
					lastOpen = -1;
					break;

				case State.OpenBrace:
					state = State.Value;
					break;

				case State.Value when c == '}' && i == logFormat.Length - 1:
					state = State.CloseBrace;
					lastClose = i;
					AddLiteral(start, lastOpen - start);
					AddValue(lastOpen, lastClose - lastOpen + 1);
					start = i + 1;
					break;

				case State.Value when c == '}':
					state = State.CloseBrace;
					lastClose = i;
					break;

				case State.CloseBrace when c == '}':
					state = State.Value;
					lastClose = -1;
					break;

				case State.CloseBrace:
					AddLiteral(start, lastOpen - start);
					AddValue(lastOpen, lastClose - lastOpen + 1);
					if (c == '{')
					{
						state = State.OpenBrace;
						lastOpen = i;
					}
					else
					{
						state = State.Literal;
						lastOpen = -1;
					}
					lastClose = -1;
					start = i;
					break;
			}
		}

		AddLiteral(start, logFormat.Length - start);

		void AddLiteral(int index, int length)
		{
			if (length == 0)
				return;

			output.Add(MessageToken.FromLiteral(logFormat.Substring(index, length)));
		}

		void AddValue(int index, int length)
		{
			if (length == 0)
				return;

			output.Add(MessageToken.FromFormat(logFormat.Substring(index, length)));
		}
	}
}
