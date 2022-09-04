namespace TinyLogger.Tokenizers;

public interface IMessageTokenizer
{
	void Tokenize<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter, IList<MessageToken> output);
	void Tokenize(IReadOnlyDictionary<string, object?> data, IList<MessageToken> output);
	void Tokenize(IReadOnlyList<MessageToken> messageTokens, IReadOnlyDictionary<string, object?> data, IList<MessageToken> output);
}
