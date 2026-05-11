using System.Text;

namespace TinyLogger.Formatters;

/// <summary>
/// Formats log messages in plain text by concatenating the message tokens without any additional formatting.
/// </summary>
public class PlainTextLogFormatter : ILogFormatter
{
	public static readonly PlainTextLogFormatter Instance = new();

	public void Format(TokenizedMessage message, StringBuilder stringBuilder)
	{
		if (message is null || stringBuilder is null)
		{
			return;
		}

		for (var i = 0; i < message.MessageTokens.Count; i++)
		{
			var token = message.MessageTokens[i];

			token.Write(stringBuilder);
		}
	}
}
