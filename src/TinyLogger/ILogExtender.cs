namespace TinyLogger;

public interface ILogExtender
{
	/// <summary>
	/// <para>Add or modify fields that can be used in the message template when tokenizing log messages.</para>
	/// <para>Store a Func of object if it is a value that is expensive to calculate or if it may not be used.</para>
	/// </summary>
	/// <param name="data">A dictionary with the data that is available.</param>
	void Extend(Dictionary<string, MessageToken?> data);
}
