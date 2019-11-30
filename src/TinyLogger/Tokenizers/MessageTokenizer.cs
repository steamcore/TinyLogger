using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyLogger.Tokenizers
{
	public class MessageTokenizer : IMessageTokenizer
	{
		private readonly IObjectTokenizer? objectTokenizer;
		private readonly IReadOnlyList<MessageToken> tokenizedMessageTemplate;

		public MessageTokenizer(string messageTemplate, IObjectTokenizer? objectTokenizer)
		{
			this.objectTokenizer = objectTokenizer;

			tokenizedMessageTemplate = TemplateTokenizer.Tokenize(messageTemplate).ToList();
		}

		public IReadOnlyList<MessageToken> Tokenize<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (TryGetDictionary(state, out var values) && values != null && values.Count > 1 && values.ContainsKey("{OriginalFormat}") && values["{OriginalFormat}"] is string originalFormat)
			{
				return Tokenize(TemplateTokenizer.Tokenize(originalFormat), values);
			}

			return new[] { MessageToken.FromLiteral(formatter(state, exception)) };

			static bool TryGetDictionary(TState state, out IReadOnlyDictionary<string, object?>? dictionary)
			{
				if (state is IReadOnlyList<KeyValuePair<string, object?>> fields)
				{
					dictionary = fields.ToDictionary(x => x.Key, x => x.Value);
					return true;
				}

				dictionary = null;
				return false;
			}
		}

		public IReadOnlyList<MessageToken> Tokenize(IReadOnlyDictionary<string, object?> data)
		{
			return Tokenize(tokenizedMessageTemplate, data);
		}

		public IReadOnlyList<MessageToken> Tokenize(IEnumerable<MessageToken> messageTokens, IReadOnlyDictionary<string, object?> data)
		{
			var result = new List<MessageToken>();

			foreach (var messageToken in messageTokens)
			{
				if (messageToken.Type == MessageTokenType.LiteralToken)
				{
					result.Add(messageToken);
				}
				else if (messageToken.Value is string key && data.ContainsKey(key))
				{
					AddTokens(objectTokenizer, result, messageToken, data[key]);
				}
			}

			return result;
		}

		private static void AddTokens(IObjectTokenizer? objectTokenizer, List<MessageToken> result, MessageToken messageToken, object? value)
		{
			if (value is null)
			{
				return;
			}

			if (value is Func<object> valueFunc)
			{
				value = valueFunc();
			}

			var hasFormat = messageToken.Alignment != null || messageToken.Format != null;

			if (value is IReadOnlyList<MessageToken> valueTokens)
			{
				foreach (var token in valueTokens)
				{
					result.Add(hasFormat ? token.WithFormat(messageToken) : token);
				}
				return;
			}

			if (value is MessageToken valueToken)
			{
				result.Add(hasFormat ? valueToken.WithFormat(messageToken) : valueToken);
				return;
			}

			var tokenized = objectTokenizer?.Tokenize(value);

			if (tokenized == null || tokenized == value)
			{
				result.Add(MessageToken.FromObject(value, messageToken.Alignment, messageToken.Format));
			}
			else
			{
				AddTokens(objectTokenizer, result, messageToken, tokenized);
			}
		}
	}
}
