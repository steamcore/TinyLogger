using System.Collections;
#if NET
using System.Runtime.CompilerServices;
#endif

namespace TinyLogger.Tokenizers
{
	public class DefaultObjectTokenizer : IObjectTokenizer
	{
		public virtual object Tokenize(object value)
		{
			return value switch
			{
				string str => str,
				IDictionary dictionary => TokenizeDictionary(dictionary),
				IEnumerable enumerable => TokenizeEnumerable(enumerable),
#if NET
				ITuple tuple => TokenizeTuple(tuple),
#endif
				_ => value
			};
		}

		private static IReadOnlyList<MessageToken> TokenizeEnumerable(IEnumerable enumerable)
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

		private static IReadOnlyList<MessageToken> TokenizeDictionary(IDictionary dictionary)
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
		private static IReadOnlyList<MessageToken> TokenizeTuple(ITuple tuple)
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
}
