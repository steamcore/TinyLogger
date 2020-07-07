using System;
using System.Collections;
using System.Collections.Generic;

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
				if (key == null)
					continue;

				var dictionaryValue = dictionary[key];
				result.Add(MessageToken.FromLiteral(separator + $"- {key}: "));
				result.Add(MessageToken.FromObject(dictionaryValue));
			}

			return result;
		}
	}
}
