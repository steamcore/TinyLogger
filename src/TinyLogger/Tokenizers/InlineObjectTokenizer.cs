using System.Collections;
#if NET
using System.Runtime.CompilerServices;
#endif

namespace TinyLogger.Tokenizers;

public class InlineObjectTokenizer : IObjectTokenizer
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

			IDictionary d => TokenizeValue(d),
			IEnumerable e => TokenizeValue(e),
#if NET
			ITuple t => TokenizeValue(t),
#endif
			_ => null
		};
	}

	internal static IReadOnlyList<MessageToken>? TokenizeValue(IDictionary dictionary)
	{
		var separator = "";
		var result = new List<MessageToken>();

		foreach (var key in dictionary.Keys)
		{
			if (key is null)
				continue;

			var dictionaryValue = dictionary[key];
			result.Add(MessageToken.FromLiteral(separator + $"{{{key}, "));
			result.Add(MessageToken.FromObject(dictionaryValue));
			result.Add(MessageToken.FromLiteral("}"));
			separator = ", ";
		}

		return result;
	}

	internal static IReadOnlyList<MessageToken>? TokenizeValue(IEnumerable enumerable)
	{
		var separator = string.Empty;
		var result = new List<MessageToken>
		{
			MessageToken.FromLiteral("[")
		};

		foreach (var item in enumerable)
		{
			result.Add(MessageToken.FromLiteral(separator));

			if (item is IGrouping<object, object> grouping)
			{
				result.Add(MessageToken.FromLiteral($"{{{grouping.Key}, "));
				if (TokenizeValue(grouping) is IReadOnlyList<MessageToken> groupItems)
				{
					result.AddRange(groupItems);
				}
				result.Add(MessageToken.FromLiteral("}"));
			}
			else
			{
				result.Add(MessageToken.FromObject(item));
			}

			separator = ", ";
		}

		result.Add(MessageToken.FromLiteral("]"));

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
}
