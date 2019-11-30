using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TinyLogger.Extenders;
using TinyLogger.Tokenizers;

namespace TinyLogger
{
	public enum QueueDepthExceededBehavior
	{
		/// <summary>
		/// Wait for up to 5 seconds to let the log renderers to catch up, then add messages to the queue anyway.
		/// </summary>
		KeepAll,

		/// <summary>
		/// For log levels of warning and up this is the same as KeepAll, but discard the rest.
		/// </summary>
		KeepWarningsAndErrors,

		/// <summary>
		/// Drop all messages until there is room in the queue.
		/// </summary>
		DiscardAll
	}

	[SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Options class, they should be modified")]
	public class TinyLoggerOptions
	{
		/// <summary>
		/// Add log extenders to add data to a dictionary that is used when rending the Template.
		/// </summary>
		public List<ILogExtender> Extenders { get; set; } = new List<ILogExtender>
		{
			new LogLevelExtender(),
			new ProcessExtender(),
			new ThreadExtender()
		};

		/// <summary>
		/// Maximum number of log messages that can be queued up to be rendered if log messages come in faster than
		/// they can be rendered.
		///
		/// Modify QueueDepthExceededBehavior to control what to do when this limit is reached.
		/// </summary>
		public int MaxQueueDepth { get; set; } = 100;

		/// <summary>
		/// Specifies which object tokenizer to use, set to null to disable.
		/// </summary>
		public IObjectTokenizer? ObjectTokenizer { get; set; } = new DefaultObjectTokenizer();

		/// <summary>
		/// If log messages are generated faster than they can be rendered a decision must be made whether to wait
		/// and block the thread until the log renderes have caught up or to start discarding log messages.
		/// </summary>
		public QueueDepthExceededBehavior QueueDepthExceededBehavior { get; set; } = QueueDepthExceededBehavior.KeepAll;

		/// <summary>
		/// The list of renderers to be used, for example ConsoleRenderer and FileRenderer.
		/// </summary>
		public List<ILogRenderer> Renderers { get; set; } = new List<ILogRenderer>();

		/// <summary>
		/// Defines the structure of every log message, see MessageTemplates for examples.
		/// </summary>
		public string Template { get; set; } = MessageTemplates.Default;
	}
}
