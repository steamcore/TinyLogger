using System.Diagnostics.CodeAnalysis;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace TinyLogger.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Must not be static.")]
public class PoolingBenchmarks
{
	[Benchmark]
	public void PlainMetadataDictionary()
	{
		_ = new Dictionary<string, object?>
		{
			["bar"] = "foo",
			["foo"] = "bar",
			["fee"] = "fum"
		};
	}

	[Benchmark]
	public void PooledMetadataDictionary()
	{
		using var pooledValue = Pooling.RentMetadataDictionary();
		pooledValue.Value["bar"] = new LiteralToken("foo");
		pooledValue.Value["foo"] = new LiteralToken("bar");
		pooledValue.Value["fee"] = new LiteralToken("fum");
	}

	[Benchmark]
	public void PlainMessageTokenList()
	{
		_ = new List<MessageToken>
		{
			new LiteralToken("foo"),
			new LiteralToken("bar"),
			new LiteralToken("fee")
		};
	}

	[Benchmark]
	public void PooledMessageTokenList()
	{
		using var pooledValue = Pooling.RentMessageTokenList();
		pooledValue.Value.Add(new LiteralToken("foo"));
		pooledValue.Value.Add(new LiteralToken("bar"));
		pooledValue.Value.Add(new LiteralToken("fee"));
	}

	[Benchmark]
	public void PlainStringBuilder()
	{
		var value = new StringBuilder();
		value.Append("foo");
		value.Append("bar");
		value.Append("fee");
	}

	[Benchmark]
	public void PooledStringBuilder()
	{
		using var pooledValue = Pooling.RentStringBuilder();
		pooledValue.Value.Append("foo");
		pooledValue.Value.Append("bar");
		pooledValue.Value.Append("fee");
	}
}
