using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TinyLogger
{
	public class TokenizedMessage
	{
		private readonly Func<IReadOnlyList<MessageToken>> getMessageTokens;
		private IReadOnlyList<MessageToken>? messageTokens;

		public LogLevel LogLevel { get; }
		public IReadOnlyList<MessageToken> MessageTokens => messageTokens ?? (messageTokens = getMessageTokens());

		public TokenizedMessage(LogLevel logLevel, Func<IReadOnlyList<MessageToken>> getMessageTokens)
		{
			LogLevel = logLevel;

			this.getMessageTokens = getMessageTokens;
		}
	}
}
