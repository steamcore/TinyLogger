using System.Collections;
#if NET
using System.Runtime.CompilerServices;
#endif

namespace TinyLogger.Tokenizers;

public class DefaultObjectTokenizer : IObjectTokenizer
{
	public virtual bool TryToTokenize(object? value, IList<MessageToken> output)
	{
		return AttemptTokenization(value, output);
	}

	internal static bool AttemptTokenization(object? value, IList<MessageToken> output)
	{
		return value switch
		{
			// Special case for strings because strings implement IEnumerable
			string => false,

			IDictionary d => TokenizeValue(d, output),
			IReadOnlyList<object> l => TokenizeValue(l, output),
			IEnumerable e => TokenizeValue(e, output),
#if NET
			ITuple t => TokenizeValue(t, output),
#endif
			_ => false
		};
	}

	internal static bool TokenizeValue(IDictionary dictionary, IList<MessageToken> output)
	{
		var separator = Environment.NewLine;

		foreach (var key in dictionary.Keys)
		{
			if (key is null)
			{
				continue;
			}

			var dictionaryValue = dictionary[key];
			output.Add(new LiteralToken(separator + $"- {key}: "));
			output.Add(new ObjectToken<object>(dictionaryValue));
		}

		return true;
	}

	internal static bool TokenizeValue(IReadOnlyList<object> list, IList<MessageToken> output)
	{
		var separator = Environment.NewLine;

		for (var i = 0; i < list.Count; i++)
		{
			var item = list[i];

			output.Add(new LiteralToken(separator + "- "));

			if (item is IGrouping<object, object> grouping)
			{
				output.Add(new LiteralToken($"{grouping.Key}: "));
			}

			if (!InlineObjectTokenizer.AttemptTokenization(item, output))
			{
				output.Add(new ObjectToken<object>(item));
			}
		}

		return true;
	}

	internal static bool TokenizeValue(IEnumerable enumerable, IList<MessageToken> output)
	{
		var separator = Environment.NewLine;

		foreach (var item in enumerable)
		{
			output.Add(new LiteralToken(separator + "- "));

			if (item is IGrouping<object, object> grouping)
			{
				output.Add(new LiteralToken($"{grouping.Key}: "));
			}

			if (!InlineObjectTokenizer.AttemptTokenization(item, output))
			{
				output.Add(new ObjectToken<object>(item));
			}
		}

		return true;
	}

#if NET
	internal static bool TokenizeValue(ITuple tuple, IList<MessageToken> output)
	{
		output.Add(new LiteralToken("("));

		for (var i = 0; i < tuple.Length; i++)
		{
			if (i > 0)
			{
				output.Add(new LiteralToken(", "));
			}

			output.Add(new ObjectToken<object?>(tuple[i]));
		}

		output.Add(new LiteralToken(")"));

		return true;
	}
#endif
}
