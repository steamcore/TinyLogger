using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TinyLogger;

/// <summary>
/// <para>Represents a limited size cache that stores key-value pairs.</para>
/// <para>The cache has a maximum number of items it can hold, and when the number of items exceeds the maximum, it automatically prunes the least recently accessed items.</para>
/// </summary>
/// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
/// <typeparam name="TValue">The type of the values in the cache.</typeparam>
public class LimitedSizeCache<TKey, TValue>
	where TKey : class
	where TValue : class
{
	private readonly ConcurrentDictionary<TKey, (TValue Value, DateTime LastAccessed)> cache;
	private readonly int maxItems;
	private readonly double pruneRatio;
	private readonly Lock pruneLock = new();

	private Task? pruneTask;

	/// <summary>
	/// Initializes a new instance of the <see cref="LimitedSizeCache{TKey, TValue}"/> class with the specified maximum number of items and prune ratio.
	/// </summary>
	/// <param name="maxItems">The maximum number of items the cache can hold.</param>
	/// <param name="pruneRatio">The ratio of items to prune when the cache exceeds the maximum number of items. Default is 0.3.</param>
	public LimitedSizeCache(int maxItems, double pruneRatio = 0.3)
	{
		Debug.Assert(maxItems > 0);

		this.maxItems = maxItems;
		this.pruneRatio = pruneRatio;
		cache = new ConcurrentDictionary<TKey, (TValue, DateTime)>();
	}

	/// <summary>
	/// Adds or updates a key-value pair in the cache.
	/// </summary>
	/// <param name="key">The key of the item to add or update.</param>
	/// <param name="value">The value of the item to add or update.</param>
	public void Put(TKey key, TValue value)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (value is null)
		{
			return;
		}

		cache[key] = (value, DateTime.UtcNow);

		if (cache.Count > maxItems && pruneTask is null)
		{
			lock (pruneLock)
			{
				pruneTask ??= Task.Run(PruneCache);
			}
		}
	}

	/// <summary>
	/// Tries to get the value associated with the specified key from the cache.
	/// </summary>
	/// <param name="key">The key of the item to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, null.</param>
	/// <returns>true if the cache contains an item with the specified key; otherwise, false.</returns>
	public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
	{
		ArgumentNullException.ThrowIfNull(key);

		if (cache.TryGetValue(key, out var item))
		{
			item.LastAccessed = DateTime.UtcNow;
			value = item.Value;
			return true;
		}

		value = null;
		return false;
	}

	private void PruneCache()
	{
		var items = new List<(TKey Key, DateTime LastAccessed)>();

		// Safely iterate items in the cache
		foreach (var item in cache)
		{
			items.Add((item.Key, item.Value.LastAccessed));
		}

		// Remove least recently accessed items
		foreach (var (Key, _) in items.OrderBy(x => x.LastAccessed).Take((int)(items.Count * pruneRatio)))
		{
			cache.TryRemove(Key, out _);
		}

		pruneTask = null;
	}
}
