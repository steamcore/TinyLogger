namespace TinyLogger;

internal sealed class NullDisposable : IDisposable, IAsyncDisposable
{
	public static readonly IDisposable Instance = new NullDisposable();

	private NullDisposable()
	{
	}

	public void Dispose()
	{
	}

	public ValueTask DisposeAsync()
	{
		return new ValueTask();
	}
}
