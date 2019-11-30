namespace TinyLogger
{
	public static class MessageTemplates
	{
		/// <summary>
		/// Default log message template
		/// </summary>
		public const string Default =
			"{logLevel_short} {categoryName} {eventId}{newLine}"
			+ "{message}{newLine}"
			+ "{exception_newLine}{exception}{exception_newLine}"
			+ "{newLine}";

		/// <summary>
		/// Log message with UTC timestamp
		/// </summary>
		public const string DefaultTimestamped =
			"{timestamp_utc} {logLevel_short} {categoryName} {eventId}{newLine}"
			+ "{message}{newLine}"
			+ "{exception_newLine}{exception}{exception_newLine}"
			+ "{newLine}";

		/// <summary>
		/// Minimal log message
		///
		/// Combine with InlineObjectTokenizer for best effect
		/// </summary>
		public const string Minimal =
			"{logLevel_short} {message} {exception_message}{newLine}";

		/// <summary>
		/// Minimal log message with UTC timestamp
		///
		/// Combine with InlineObjectTokenizer for best effect
		/// </summary>
		public const string MinimalTimestamped =
			"{timestamp_utc} {logLevel_short} {message} {exception_message}{newLine}";

		/// <summary>
		/// Approximates the format output by Microsoft.Extensions.Logging.Console
		///
		/// Set ObjectTokenizer to InlineObjectTokenizer and implement a bland color theme for the full experience.
		/// </summary>
		public const string NetCoreConsole =
			"{logLevel_short}: {categoryName}[{eventId}]{newLine}"
			+ "      {message}{newLine}"
			+ "{exception}{exception_newLine}";
	}
}
