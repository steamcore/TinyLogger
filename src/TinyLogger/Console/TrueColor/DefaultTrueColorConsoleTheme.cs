using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace TinyLogger.Console.TrueColor;

[SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Theme colors")]
[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Theme colors")]
[SuppressMessage("Redundancy", "RCS1213:Remove unused member declaration.", Justification = "Theme colors")]
public class DefaultTrueColorConsoleTheme : ITrueColorConsoleTheme
{
	private static readonly Color Black = GetColor("202020");
	private static readonly Color Red = GetColor("CE5746");
	private static readonly Color Green = GetColor("4BC54B");
	private static readonly Color Yellow = GetColor("CFBE02");
	private static readonly Color Blue = GetColor("436FE1");
	private static readonly Color Purple = GetColor("D338D3");
	private static readonly Color Cyan = GetColor("33BBC8");
	private static readonly Color White = GetColor("DBDBDB");
	private static readonly Color BrightBlack = GetColor("818383");
	private static readonly Color BrightRed = GetColor("FF604A");
	private static readonly Color BrightGreen = GetColor("3BF53B");
	private static readonly Color BrightYellow = GetColor("ECDC23");
	private static readonly Color BrightBlue = GetColor("7490FD");
	private static readonly Color BrightPurple = GetColor("F935F8");
	private static readonly Color BrightCyan = GetColor("14F0F0");
	private static readonly Color BrightWhite = GetColor("FFFFFF");

	public virtual (Color? foreground, Color? background) GetColors(object? value, LogLevel logLevel)
	{
		return (value, logLevel) switch
		{
			(bool b, _) => (b ? BrightGreen : BrightRed, null),

			(byte _, _) => (BrightPurple, null),
			(sbyte _, _) => (BrightPurple, null),
			(short _, _) => (BrightPurple, null),
			(ushort _, _) => (BrightPurple, null),
			(int _, _) => (BrightPurple, null),
			(uint _, _) => (BrightPurple, null),
			(long _, _) => (BrightPurple, null),
			(ulong _, _) => (BrightPurple, null),
			(float _, _) => (BrightPurple, null),
			(double _, _) => (BrightPurple, null),
			(decimal _, _) => (BrightPurple, null),

			(char _, _) _ => (BrightYellow, null),
			(string _, _) _ => (BrightYellow, null),

			(DateTime _, _) _ => (BrightCyan, null),
			(DateTimeOffset _, _) _ => (BrightCyan, null),
			(TimeSpan _, _) _ => (BrightCyan, null),

			(Guid _, _) _ => (Purple, null),
			(Uri _, _) _ => (BrightBlue, null),
			(Version _, _) _ => (Cyan, null),

			(Exception _, LogLevel.Warning) => (BrightYellow, null),
			(Exception _, LogLevel.Error) => (BrightRed, null),
			(Exception _, LogLevel.Critical) => (BrightRed, null),

			(EventId _, _) => (BrightBlack, null),

			(LogLevel level, _) when level == LogLevel.Trace => (BrightBlack, null),
			(LogLevel level, _) when level == LogLevel.Debug => (Blue, null),
			(LogLevel level, _) when level == LogLevel.Information => (Green, null),
			(LogLevel level, _) when level == LogLevel.Warning => (BrightYellow, null),
			(LogLevel level, _) when level == LogLevel.Error => (BrightRed, null),
			(LogLevel level, _) when level == LogLevel.Critical => (BrightWhite, Red),

			(_, _) => (null, null)
		};
	}

	private static Color GetColor(string hex)
	{
		return Color.FromArgb(int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
	}
}
