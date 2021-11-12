namespace TinyLogger.Tokenizers;

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
		var state = State.Literal;
		var start = 0;
		var lastOpen = -1;
		var lastClose = -1;
		var result = new List<MessageToken>();

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

		return result;

		void AddLiteral(int index, int length)
		{
			if (length == 0)
				return;

			result.Add(MessageToken.FromLiteral(logFormat.Substring(index, length)));
		}

		void AddValue(int index, int length)
		{
			if (length == 0)
				return;

			result.Add(MessageToken.FromFormat(logFormat.Substring(index, length)));
		}
	}
}
