namespace TinyLogger.Tokenizers;

public interface IMessageTokenizer
{
	IReadOnlyList<MessageToken> Tokenize<TState>(TState state, Exception exception, Func<TState, Exception, string> formatter);
	IReadOnlyList<MessageToken> Tokenize(IReadOnlyDictionary<string, object?> data);
	IReadOnlyList<MessageToken> Tokenize(IEnumerable<MessageToken> messageTokens, IReadOnlyDictionary<string, object?> data);
}
