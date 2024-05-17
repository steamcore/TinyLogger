namespace TinyLogger.Tokenizers;

public class TemplateTokenizerTests
{
	[Theory]
	[MemberData(nameof(TestDataKeys))]
	public void Tokenize_should_return_expected_tokens(string input)
	{
		var result = TemplateTokenizer.Tokenize(input);

		result.ShouldBe(testData[input]);
	}

	public static TheoryData<string> TestDataKeys => new(testData.Keys);

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<MessageToken>> testData = new Dictionary<string, IReadOnlyList<MessageToken>>
	{
		{
			"abc{{def}ghi",
			[
				new LiteralToken("abc{{def}ghi")
			]
		},
		{
			"abc{def}}ghi",
			[
				new LiteralToken("abc{def}}ghi")
			]
		},
		{
			"abc{{def}}ghi",
			[
				new LiteralToken("abc{{def}}ghi")
			]
		},
		{
			"abc{d ef}ghi",
			[
				new LiteralToken("abc"),
				new ObjectToken<string>("d ef"),
				new LiteralToken("ghi")
			]
		},
		{
			"abc{def}ghi",
			[
				new LiteralToken("abc"),
				new ObjectToken<string>("def"),
				new LiteralToken("ghi")
			]
		},
		{
			"{a,2}{b:o}{c,3:#}",
			[
				new ObjectToken<string>("a", Alignment: 2),
				new ObjectToken<string>("b", Format: "o"),
				new ObjectToken<string>("c", Alignment: 3, Format: "#")
			]
		}
	};
}
