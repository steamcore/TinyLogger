namespace TinyLogger.Tokenizers;

public class TemplateTokenizerTests
{
	[Test]
	[MethodDataSource(nameof(TestData))]
	public void Tokenize_should_return_expected_tokens(string input, IReadOnlyList<MessageToken> expectedTokens)
	{
		var result = TemplateTokenizer.Tokenize(input);

		result.ShouldBe(expectedTokens);
	}

	public static IEnumerable<Func<(string, IReadOnlyList<MessageToken>)>> TestData()
	{
		return [
			() => ("abc{{def}ghi", new MessageToken[]
			{
				new LiteralToken("abc{{def}ghi")
			}),
			() => ("abc{def}}ghi", new MessageToken[]
			{
				new LiteralToken("abc{def}}ghi")
			}),
			() => ("abc{{def}}ghi", new MessageToken[]
			{
				new LiteralToken("abc{{def}}ghi")
			}),
			() => ("abc{d ef}ghi", new MessageToken[]
			{
				new LiteralToken("abc"),
				new ObjectToken<string>("d ef"),
				new LiteralToken("ghi")
			}),
			() => ("abc{def}ghi", new MessageToken[]
			{
				new LiteralToken("abc"),
				new ObjectToken<string>("def"),
				new LiteralToken("ghi")
			}),
			() => ("{a,2}{b:o}{c,3:#}", new MessageToken[]
			{
				new ObjectToken<string>("a", Alignment: 2),
				new ObjectToken<string>("b", Format: "o"),
				new ObjectToken<string>("c", Alignment: 3, Format: "#")
			})
		];
	}
}
