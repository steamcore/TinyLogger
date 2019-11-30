using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.

namespace TinyLogger
{
	[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "IAsyncDisposable")]
	internal class ReactiveEnumerator<T> : IAsyncEnumerator<T>
	{
		private static readonly TimeSpan resetEventTimeout = TimeSpan.FromSeconds(5);

		private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
		private readonly ManualResetEventSlim queueResetEvent = new ManualResetEventSlim(true, 2);
		private readonly ManualResetEventSlim workerResetEvent = new ManualResetEventSlim(false, 2);
		private readonly int maxQueueDepth;
		private readonly Func<T, bool> importanceArbiter;

		private bool completed;
		private bool disposed;

		public T Current { get; private set; }

		public ReactiveEnumerator(int maxQueueDepth, Func<T, bool> importanceArbiter)
		{
			this.maxQueueDepth = maxQueueDepth;
			this.importanceArbiter = importanceArbiter;
		}

		[SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "IAsyncDisposable")]
		public async ValueTask DisposeAsync()
		{
			await DisposeAsync(true).ConfigureAwait(false);
			GC.SuppressFinalize(this);
		}

		protected virtual Task DisposeAsync(bool disposing)
		{
			if (disposed)
				return Task.CompletedTask;

			if (disposing)
			{
				workerResetEvent.Dispose();
				queueResetEvent.Dispose();
			}

			disposed = true;

			return Task.CompletedTask;
		}

		public async ValueTask<bool> MoveNextAsync()
		{
			while (!disposed)
			{
				if (MoveNext())
				{
					return true;
				}

				if (completed)
				{
					break;
				}

				if (queue.IsEmpty)
				{
					// Yield the thread so we can wait for more work on a background thread, this is important
					await Task.Yield();

					if (workerResetEvent.Wait(resetEventTimeout))
					{
						workerResetEvent.Reset();
					}
				}
			}

			Current = default;
			return false;
		}

		private bool MoveNext()
		{
			if (queue.TryDequeue(out var value))
			{
				Current = value;

				// In case queue-depth was previously exceeded callers may need to be woken up
				if (!queueResetEvent.IsSet && queue.Count < maxQueueDepth)
				{
					queueResetEvent.Set();
				}

				return true;
			}

			return false;
		}

		public void OnComplete()
		{
			completed = true;

			// Signal worker thread to wake up so it can run to completion
			workerResetEvent.Set();
		}

		public void OnNext(T value)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(ReactiveEnumerator<T>));

			if (completed)
				throw new InvalidOperationException("Enumeration is already completed");

			// Verify that queue depth is not exceeded
			if (queue.Count >= maxQueueDepth)
			{
				// Discard this message if it is not deemed important
				if (!importanceArbiter(value))
				{
					return;
				}

				// Wait for worker threads to catch up, but don't wait forever
				// or a potential deadlock will never self heal
				if (queueResetEvent.Wait(resetEventTimeout))
				{
					queueResetEvent.Reset();
				}
			}

			// Add message to the queue
			queue.Enqueue(value);

			// Signal the worker thread that there is work to be done
			if (!workerResetEvent.IsSet)
			{
				workerResetEvent.Set();
			}
		}
	}
}
