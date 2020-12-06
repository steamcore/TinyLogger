using System.Collections;
using System.Collections.Generic;

namespace TinyLogger.Tokenizers
{
	public class InlineObjectTokenizer : IObjectTokenizer
	{
		public virtual object Tokenize(object value)
		{
			return value switch
			{
				IDictionary dictionary => TokenizeDictionary(dictionary),
				ICollection collection => TokenizeCollection(collection),

				_ => value
			};
		}

		private static IReadOnlyList<MessageToken> TokenizeCollection(ICollection collection)
		{
			var separator = "";
			var result = new List<MessageToken>();

			foreach (var item in collection)
			{
				result.Add(MessageToken.FromLiteral(separator));
				result.Add(MessageToken.FromObject(item));
				separator = ", ";
			}

			return result;
		}

		private static IReadOnlyList<MessageToken> TokenizeDictionary(IDictionary dictionary)
		{
			var separator = "";
			var result = new List<MessageToken>();

			foreach (var key in dictionary.Keys)
			{
				if (key is null)
					continue;

				var dictionaryValue = dictionary[key];
				result.Add(MessageToken.FromLiteral(separator + $"[{key}, "));
				result.Add(MessageToken.FromObject(dictionaryValue));
				result.Add(MessageToken.FromLiteral("]"));
				separator = ", ";
			}

			return result;
		}
	}
}
