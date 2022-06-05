using System.Collections;
#if NET
using System.Runtime.CompilerServices;
#endif

namespace TinyLogger.Tokenizers;

public class DefaultObjectTokenizer : IObjectTokenizer
{
	public virtual object Tokenize(object value)
	{
		return AttemptTokenization(value) ?? value;
	}

	internal static IReadOnlyList<MessageToken>? AttemptTokenization(object value)
	{
		return value switch
		{
			// Special case for strings because strings implement IEnumerable
			string => null,

			_ => TokenizeValue((dynamic)value)
		};
	}

	internal static IReadOnlyList<MessageToken>? TokenizeValue(IEnumerable enumerable)
	{
		var separator = Environment.NewLine;
		var result = new List<MessageToken>();

		foreach (var item in enumerable)
		{
			result.Add(MessageToken.FromLiteral(separator + "- "));
			result.Add(MessageToken.FromObject(item));
		}

		return result;
	}

	internal static IReadOnlyList<MessageToken>? TokenizeValue(IDictionary dictionary)
	{
		var separator = Environment.NewLine;
		var result = new List<MessageToken>();

		foreach (var key in dictionary.Keys)
		{
			if (key is null)
				continue;

			var dictionaryValue = dictionary[key];
			result.Add(MessageToken.FromLiteral(separator + $"- {key}: "));
			result.Add(MessageToken.FromObject(dictionaryValue));
		}

		return result;
	}

#if NET
	internal static IReadOnlyList<MessageToken>? TokenizeValue(ITuple tuple)
	{
		var result = new List<MessageToken>
		{
			MessageToken.FromLiteral("(")
		};

		for (var i = 0; i < tuple.Length; i++)
		{
			if (i > 0)
			{
				result.Add(MessageToken.FromLiteral(", "));
			}

			result.Add(MessageToken.FromObject(tuple[i]));
		}

		result.Add(MessageToken.FromLiteral(")"));
		return result;
	}
#endif

	internal static IReadOnlyList<MessageToken>? TokenizeValue<TKey, TValue>(ILookup<TKey, TValue> lookup)
	{
		var separator = Environment.NewLine;
		var result = new List<MessageToken>();

		foreach (var item in lookup)
		{
			if (item is null)
				continue;

			result.Add(MessageToken.FromLiteral(separator + $"- {item.Key}: "));
			if (InlineObjectTokenizer.AttemptTokenization(item) is IReadOnlyList<MessageToken> tokens)
			{
				result.AddRange(tokens);
			}
			else
			{
				result.Add(MessageToken.FromObject(item));
			}
		}

		return result;
	}

	internal static IReadOnlyList<MessageToken>? TokenizeValue(object _)
	{
		return null;
	}
}
