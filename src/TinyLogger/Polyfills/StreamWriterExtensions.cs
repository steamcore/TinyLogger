#if !NET
namespace System.IO;

internal static class StreamWriterExtensions
{
	extension(StreamWriter streamWriter)
	{
		/// <summary>
		/// Polyfill for .NET Standard.
		/// </summary>
		public Task DisposeAsync()
		{
			streamWriter.Dispose();

			return Task.CompletedTask;
		}
	}
}
#endif
