using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TinyLogger
{
	internal class ReactiveEnumerable<T> : IAsyncEnumerable<T>
	{
		private readonly EnumeratorCollection enumerators = new EnumeratorCollection();
		private readonly int maxQueueDepth;
		private readonly Func<T, bool> importanceArbiter;

		public ReactiveEnumerable(int maxQueueDepth, Func<T, bool> importanceArbiter)
		{
			this.maxQueueDepth = maxQueueDepth;
			this.importanceArbiter = importanceArbiter;
		}

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			var enumerator = new ReactiveEnumerator<T>(maxQueueDepth, importanceArbiter);
			enumerators.Add(enumerator);
			return enumerator;
		}

		public void OnComplete()
		{
			var currentEnumerators = enumerators.GetAll();

			enumerators.Clear();

			for (var i = 0; i < currentEnumerators.Count; i++)
			{
				currentEnumerators[i].OnComplete();
			}
		}

		public void OnNext(T value)
		{
			var currentEnumerators = enumerators.GetAll();

			for (var i = 0; i < currentEnumerators.Count; i++)
			{
				currentEnumerators[i].OnNext(value);
			}
		}

		private class EnumeratorCollection
		{
			private readonly object _lock = new object();
			private readonly List<ReactiveEnumerator<T>> collection = new List<ReactiveEnumerator<T>>();

			private IReadOnlyList<ReactiveEnumerator<T>> readCollection = Array.Empty<ReactiveEnumerator<T>>();

			public void Clear()
			{
				lock (_lock)
				{
					collection.Clear();
					readCollection = Array.Empty<ReactiveEnumerator<T>>();
				}
			}

			public void Add(ReactiveEnumerator<T> enumerator)
			{
				lock (_lock)
				{
					collection.Add(enumerator);
					readCollection = collection.ToList();
				}
			}

			public IReadOnlyList<ReactiveEnumerator<T>> GetAll()
			{
				lock (_lock)
				{
					return readCollection;
				}
			}
		}
	}
}
