using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TinyLogger
{
	public class TokenizedMessage
	{
		private readonly Func<IReadOnlyList<MessageToken>> getMessageTokens;
		private IReadOnlyList<MessageToken>? messageTokens;

		public string CategoryName { get; }
		public LogLevel LogLevel { get; }
		public IReadOnlyList<MessageToken> MessageTokens => messageTokens ?? (messageTokens = getMessageTokens());

		public TokenizedMessage(string categoryName, LogLevel logLevel, Func<IReadOnlyList<MessageToken>> getMessageTokens)
		{
			CategoryName = categoryName;
			LogLevel = logLevel;

			this.getMessageTokens = getMessageTokens;
		}
	}
}
