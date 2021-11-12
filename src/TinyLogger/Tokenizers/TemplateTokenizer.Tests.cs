namespace TinyLogger.Tokenizers
{
	public class TemplateTokenizerTests
	{
		[Theory]
		[MemberData(nameof(GetTestData))]
		public void Tokenize_should_return_expected_tokens(string input, IReadOnlyList<MessageToken> expectedTokens)
		{
			var result = TemplateTokenizer.Tokenize(input);

			result.ShouldBe(expectedTokens);
		}

		public static IEnumerable<object[]> GetTestData()
		{
			yield return new object[]
			{
				"abc{{def}ghi",
				new List<MessageToken>
				{
					MessageToken.FromLiteral("abc{{def}ghi")
				}
			};

			yield return new object[]
			{
				"abc{def}}ghi",
				new List<MessageToken>
				{
					MessageToken.FromLiteral("abc{def}}ghi")
				}
			};

			yield return new object[]
			{
				"abc{{def}}ghi",
				new List<MessageToken>
				{
					MessageToken.FromLiteral("abc{{def}}ghi")
				}
			};

			yield return new object[]
{
				"abc{d ef}ghi",
				new List<MessageToken>
				{
					MessageToken.FromLiteral("abc"),
					MessageToken.FromObject("d ef"),
					MessageToken.FromLiteral("ghi")
				}
			};

			yield return new object[]
			{
				"abc{def}ghi",
				new List<MessageToken>
				{
					MessageToken.FromLiteral("abc"),
					MessageToken.FromObject("def"),
					MessageToken.FromLiteral("ghi")
				}
			};

			yield return new object[]
			{
				"{a,2}{b:o}{c,3:#}",
				new List<MessageToken>
				{
					MessageToken.FromObject("a", alignment: 2),
					MessageToken.FromObject("b", format: "o"),
					MessageToken.FromObject("c", alignment: 3, format: "#")
				}
			};
		}
	}
}
