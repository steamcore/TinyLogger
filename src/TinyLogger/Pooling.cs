using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace TinyLogger;

[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "This only reason this is a value type is to avoid boxing allocations.")]
public readonly struct PooledValue<T> : IDisposable
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
	private static readonly ObjectPool<Dictionary<string, MessageToken?>> metadataDictionaryPool = ObjectPool.Create(new DictionaryPoolPolicy<string, MessageToken?>(50));
	private static readonly ObjectPool<StringBuilder> stringBuilderPool = ObjectPool.Create(new StringBuilderPooledObjectPolicy());
	private static readonly ObjectPool<List<MessageToken>> messageTokenListPool = ObjectPool.Create(new ListPoolPolicy<MessageToken>(50));

	public static PooledValue<Dictionary<string, MessageToken?>> RentMetadataDictionary() => new(metadataDictionaryPool);
	public static PooledValue<List<MessageToken>> RentMessageTokenList() => new(messageTokenListPool);
	public static PooledValue<StringBuilder> RentStringBuilder() => new(stringBuilderPool);

	private sealed class DictionaryPoolPolicy<TKey, TValue>(int capacity) :
		IPooledObjectPolicy<Dictionary<TKey, TValue>>
		where TKey : notnull
	{
		public Dictionary<TKey, TValue> Create()
		{
			return new Dictionary<TKey, TValue>(capacity);
		}

		public bool Return(Dictionary<TKey, TValue> obj)
		{
			obj.Clear();
			return true;
		}
	}

	private sealed class ListPoolPolicy<T>(int capacity) :
		IPooledObjectPolicy<List<T>>
	{
		public List<T> Create()
		{
			return new List<T>(capacity);
		}

		public bool Return(List<T> obj)
		{
			obj.Clear();
			return true;
		}
	}
}
