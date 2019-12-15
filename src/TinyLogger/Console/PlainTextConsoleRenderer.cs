using System;
using System.Text;
using System.Threading.Tasks;
using SystemConsole = System.Console;

namespace TinyLogger.Console
{
	/// <summary>
	/// Renders log messages to the console in plain text.
	/// </summary>
	public class PlainTextConsoleRenderer : ILogRenderer
	{
		public Task Render(TokenizedMessage message)
		{
			var sb = new StringBuilder(128);

			foreach (var token in message.MessageTokens)
			{
				sb.Append(token.ToString());
			}

			SystemConsole.Write(sb.ToString());

			return Task.CompletedTask;
		}
	}
}
