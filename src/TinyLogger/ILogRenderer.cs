using System.Threading.Tasks;

namespace TinyLogger
{
	public interface ILogRenderer
	{
		/// <summary>
		/// Render a tokenized message to some medium, like the console or a file.
		/// </summary>
		/// <param name="message">Callback to tokenize and retreive a log message.</param>
		/// <returns>Async Task</returns>
		Task Render(TokenizedMessage message);
	}
}
