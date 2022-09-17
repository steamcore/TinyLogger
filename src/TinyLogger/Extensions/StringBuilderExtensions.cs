using System.Buffers;
using SystemConsole = System.Console;

namespace System.Text;

internal static class StringBuilderExtensions
{
	internal static void WriteToConsole(this StringBuilder sb)
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

	internal static async Task WriteToStreamWriterAsync(this StringBuilder sb, StreamWriter sw)
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
