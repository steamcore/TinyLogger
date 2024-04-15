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
				MessageToken.FromLiteral("abc{{def}ghi")
			]
		},
		{
			"abc{def}}ghi",
			[
				MessageToken.FromLiteral("abc{def}}ghi")
			]
		},
		{
			"abc{{def}}ghi",
			[
				MessageToken.FromLiteral("abc{{def}}ghi")
			]
		},
		{
			"abc{d ef}ghi",
			[
				MessageToken.FromLiteral("abc"),
				MessageToken.FromObject("d ef"),
				MessageToken.FromLiteral("ghi")
			]
		},
		{
			"abc{def}ghi",
			[
				MessageToken.FromLiteral("abc"),
				MessageToken.FromObject("def"),
				MessageToken.FromLiteral("ghi")
			]
		},
		{
			"{a,2}{b:o}{c,3:#}",
			[
				MessageToken.FromObject("a", alignment: 2),
				MessageToken.FromObject("b", format: "o"),
				MessageToken.FromObject("c", alignment: 3, format: "#")
			]
		}
	};
}
