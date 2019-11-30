using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TinyLogger
{
	public class TokenizedMessage
	{
		public LogLevel LogLevel { get; }
		public IReadOnlyList<MessageToken> Message { get; }

		public TokenizedMessage(LogLevel logLevel, IReadOnlyList<MessageToken> message)
		{
			LogLevel = logLevel;
			Message = message;
		}
	}
}
