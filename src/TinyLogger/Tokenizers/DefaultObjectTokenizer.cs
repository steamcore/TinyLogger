using System.Collections;
#if NET
using System.Runtime.CompilerServices;
#endif

namespace TinyLogger.Tokenizers;

public class DefaultObjectTokenizer : IObjectTokenizer
{
	public virtual bool TryToTokenize(object value, IList<MessageToken> output)
	{
		return AttemptTokenization(value, output);
	}

	internal static bool AttemptTokenization(object value, IList<MessageToken> output)
	{
		return value switch
		{
			// Special case for strings because strings implement IEnumerable
			string => false,

			IDictionary d => TokenizeValue(d, output),
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
				continue;

			var dictionaryValue = dictionary[key];
			output.Add(MessageToken.FromLiteral(separator + $"- {key}: "));
			output.Add(MessageToken.FromObject(dictionaryValue));
		}

		return true;
	}

	internal static bool TokenizeValue(IEnumerable enumerable, IList<MessageToken> output)
	{
		var separator = Environment.NewLine;

		foreach (var item in enumerable)
		{
			output.Add(MessageToken.FromLiteral(separator + "- "));

			if (item is IGrouping<object, object> grouping)
			{
				output.Add(MessageToken.FromLiteral($"{grouping.Key}: "));
			}

			if (!InlineObjectTokenizer.AttemptTokenization(item, output))
			{
				output.Add(MessageToken.FromObject(item));
			}
		}

		return true;
	}

#if NET
	internal static bool TokenizeValue(ITuple tuple, IList<MessageToken> output)
	{
		output.Add(MessageToken.FromLiteral("("));

		for (var i = 0; i < tuple.Length; i++)
		{
			if (i > 0)
			{
				output.Add(MessageToken.FromLiteral(", "));
			}

			output.Add(MessageToken.FromObject(tuple[i]));
		}

		output.Add(MessageToken.FromLiteral(")"));

		return true;
	}
#endif
}
