using System.Buffers;
using SystemConsole = System.Console;

namespace System.Text;

internal static class StringBuilderExtensions
{
	extension(StringBuilder sb)
	{
		internal void WriteToConsole()
		{
			var buffer = ArrayPool<char>.Shared.Rent(sb.Length);

			try
			{
				sb.CopyTo(0, buffer, 0, sb.Length);

				SystemConsole.Write(buffer, 0, sb.Length);
			}
			finally
			{
				ArrayPool<char>.Shared.Return(buffer);
			}
		}

		internal async Task WriteToStreamWriterAsync(StreamWriter sw)
		{
			var buffer = ArrayPool<char>.Shared.Rent(sb.Length);

			try
			{
				sb.CopyTo(0, buffer, 0, sb.Length);

				await sw.WriteAsync(buffer, 0, sb.Length).ConfigureAwait(false);
			}
			finally
			{
				ArrayPool<char>.Shared.Return(buffer);
			}
		}
	}
}
