using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TinyLogger
{
	public class ReactiveEnumerableTests
	{
		[Fact]
		public async Task All_values_should_be_enumerated()
		{
			var enumerable = new ReactiveEnumerable<Guid>(500, value => true);

			var generated = new List<Guid>();
			var actual = new List<Guid>();

			var worker = Worker(enumerable, actual);

			for (var i = 0; i < 100; i++)
			{
				var guid = Guid.NewGuid();
				generated.Add(guid);
				enumerable.OnNext(guid);
			}

			enumerable.OnComplete();

			await worker;

			Assert.Equal(generated, actual);
		}

		[Fact]
		public async Task All_values_should_be_enumerated_even_if_queueDepth_is_small()
		{
			var enumerable = new ReactiveEnumerable<Guid>(10, value => true);

			var generated = new List<Guid>();
			var actual = new List<Guid>();

			var worker = Worker(enumerable, actual);

			for (var i = 0; i < 10_000; i++)
			{
				var guid = Guid.NewGuid();
				generated.Add(guid);
				enumerable.OnNext(guid);
			}

			enumerable.OnComplete();

			await worker;

			Assert.Equal(generated, actual);
		}

		private async Task Worker<T>(IAsyncEnumerable<T> enumerable, List<T> result)
		{
			await foreach (var guid in enumerable)
			{
				result.Add(guid);
			}
		}
	}
}
