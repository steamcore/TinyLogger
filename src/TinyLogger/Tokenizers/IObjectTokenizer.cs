namespace TinyLogger.Tokenizers;

public interface IObjectTokenizer
{
	bool TryToTokenize(object? value, IList<MessageToken> output);
}
