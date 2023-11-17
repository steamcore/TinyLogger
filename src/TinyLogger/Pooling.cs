using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace TinyLogger;

public interface IPooledValue<T> : IDisposable
{
	T Value { get; }
}

public static class Pooling
{
	private static readonly ObjectPool<Dictionary<string, object?>> metadataDictionaryPool = ObjectPool.Create(new DictionaryPoolPolicy());
	private static readonly ObjectPool<StringBuilder> stringBuilderPool = ObjectPool.Create(new StringBuilderPooledObjectPolicy());
	private static readonly ObjectPool<List<MessageToken>> messageTokenListPool = ObjectPool.Create(new ListPoolPolicy<MessageToken>());

	public static IPooledValue<Dictionary<string, object?>> RentMetadataDictionary() => new PooledValue<Dictionary<string, object?>>(metadataDictionaryPool);
	public static IPooledValue<List<MessageToken>> RentMessageTokenList() => new PooledValue<List<MessageToken>>(messageTokenListPool);
	public static IPooledValue<StringBuilder> RentStringBuilder() => new PooledValue<StringBuilder>(stringBuilderPool);

	private class DictionaryPoolPolicy : IPooledObjectPolicy<Dictionary<string, object?>>
	{
		public Dictionary<string, object?> Create()
		{
			return [];
		}

		public bool Return(Dictionary<string, object?> obj)
		{
			obj.Clear();
			return true;
		}
	}

	private class ListPoolPolicy<T> : IPooledObjectPolicy<List<T>>
	{
		public List<T> Create()
		{
			return [];
		}

		public bool Return(List<T> obj)
		{
			obj.Clear();
			return true;
		}
	}

	private class PooledValue<T>(ObjectPool<T> pool) : IPooledValue<T>
		where T : class
	{
		public T Value { get; } = pool.Get();

		public void Dispose()
		{
			pool.Return(Value);
		}
	}
}
