using System.Diagnostics.CodeAnalysis;
using TinyLogger.Extenders;
using TinyLogger.Tokenizers;

namespace TinyLogger;

public class TinyLoggerOptions
{
	/// <summary>
	/// Add log extenders to add data to a dictionary that is used when rending the Template.
	/// </summary>
	[SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "It's supposed to be configurable")]
	[SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "It's supposed to be configurable")]
	public List<ILogExtender> Extenders { get; set; } =
	[
		new LogLevelExtender(),
		new ProcessExtender(),
		new ThreadExtender()
	];

	/// <summary>
	/// <para>
	/// If log messages are generated faster than they can be rendered a decision must be made whether to wait
	/// and block the thread until the log renderes have caught up or to start discarding log messages.
	/// </para>
	/// <para>Return true to block thread and keep message, false to discard message and move along.</para>
	/// </summary>
	public Func<TokenizedMessage, bool> BackPressureArbiter { get; set; } = _ => true;

	/// <summary>
	/// <para>
	/// Maximum number of log messages that can be queued up to be rendered if log messages come in faster than
	/// they can be rendered.
	/// </para>
	/// <para>Use BackPressureArbiter options to control what to do when this limit is reached.</para>
	/// </summary>
	public int MaxQueueDepth { get; set; } = 100;

	/// <summary>
	/// Specifies which object tokenizer to use, set to null to disable.
	/// </summary>
	public IObjectTokenizer? ObjectTokenizer { get; set; } = new DefaultObjectTokenizer();

	/// <summary>
	/// The list of renderers to be used, for example ConsoleRenderer and FileRenderer.
	/// </summary>
	[SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "It's supposed to be configurable")]
	[SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "It's supposed to be configurable")]
	public List<ILogRenderer> Renderers { get; set; } = [];

	/// <summary>
	/// Defines the structure of every log message, see MessageTemplates for examples.
	/// </summary>
	public string Template { get; set; } = MessageTemplates.Default;

	/// <summary>
	/// Disables the asynchronous channel and uses synchronous writes instead which is useful when doing benchmarks.
	/// </summary>
	public bool UseSynchronousWrites { get; set; }
}
