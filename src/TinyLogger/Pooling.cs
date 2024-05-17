using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace TinyLogger;

[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "This only reason this is a value type is to avoid boxing allocations.")]
public struct PooledValue<T> : IDisposable
	where T : class
{
	private readonly ObjectPool<T> pool;

	public T Value { get; }

	internal PooledValue(ObjectPool<T> pool)
	{
		this.pool = pool;

		Value = pool.Get();
	}

	public void Dispose()
	{
		pool.Return(Value);
	}
}

public static class Pooling
{
	private static readonly ObjectPool<Dictionary<string, object?>> metadataDictionaryPool = ObjectPool.Create(new DictionaryPoolPolicy());
	private static readonly ObjectPool<StringBuilder> stringBuilderPool = ObjectPool.Create(new StringBuilderPooledObjectPolicy());
	private static readonly ObjectPool<List<MessageToken>> messageTokenListPool = ObjectPool.Create(new ListPoolPolicy<MessageToken>());

	public static PooledValue<Dictionary<string, object?>> RentMetadataDictionary() => new PooledValue<Dictionary<string, object?>>(metadataDictionaryPool);
	public static PooledValue<List<MessageToken>> RentMessageTokenList() => new PooledValue<List<MessageToken>>(messageTokenListPool);
	public static PooledValue<StringBuilder> RentStringBuilder() => new PooledValue<StringBuilder>(stringBuilderPool);

	private sealed class DictionaryPoolPolicy : IPooledObjectPolicy<Dictionary<string, object?>>
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

	private sealed class ListPoolPolicy<T> : IPooledObjectPolicy<List<T>>
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
}
