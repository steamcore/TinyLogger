using System.Collections.Generic;

namespace TinyLogger
{
	public interface ILogExtender
	{
		/// <summary>
		/// Add or modify fields that can be used in the message template when tokenizing log messages.
		///
		/// Store a Func of object if it is a value that is expensive to calculate or if it may not be used.
		/// </summary>
		/// <param name="data">A dictionary with the data that is available.</param>
		void Extend(Dictionary<string, object?> data);
	}
}
