using System.Text;

namespace TinyLogger.Formatters;

public interface ILogFormatter
{
	/// <summary>
	/// Formats a log message.
	/// </summary>
	/// <param name="message">The log message to format.</param>
	/// <param name="stringBuilder">The <see cref="StringBuilder"/> to write the formatted log message to.</param>
	/// <returns>The formatted log message.</returns>
	void Format(TokenizedMessage message, StringBuilder stringBuilder);
}
