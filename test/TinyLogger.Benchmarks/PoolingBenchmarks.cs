using System.Diagnostics.CodeAnalysis;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace TinyLogger.Benchmarks;

[MemoryDiagnoser]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Must not be static.")]
public class PoolingBenchmarks
{
	[Benchmark]
	public void PlainMetadataDictionary()
	{
		var value = new Dictionary<string, object?>
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
		pooledValue.Value["bar"] = "foo";
		pooledValue.Value["foo"] = "bar";
		pooledValue.Value["fee"] = "fum";
	}

	[Benchmark]
	public void PlainMessageTokenList()
	{
		var value = new List<MessageToken>
		{
			new MessageToken("foo", MessageTokenType.LiteralToken),
			new MessageToken("bar", MessageTokenType.LiteralToken),
			new MessageToken("fee", MessageTokenType.LiteralToken)
		};
	}

	[Benchmark]
	public void PooledMessageTokenList()
	{
		using var pooledValue = Pooling.RentMessageTokenList();
		pooledValue.Value.Add(new MessageToken("foo", MessageTokenType.LiteralToken));
		pooledValue.Value.Add(new MessageToken("bar", MessageTokenType.LiteralToken));
		pooledValue.Value.Add(new MessageToken("fee", MessageTokenType.LiteralToken));
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
