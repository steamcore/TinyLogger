using System.Drawing;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace TinyLogger.Console.TrueColor;

public class DefaultTrueColorConsoleTheme : ITrueColorConsoleTheme
{
	public static readonly Color Black = GetColor("202020");
	public static readonly Color Red = GetColor("CE5746");
	public static readonly Color Green = GetColor("4BC54B");
	public static readonly Color Yellow = GetColor("CFBE02");
	public static readonly Color Blue = GetColor("436FE1");
	public static readonly Color Purple = GetColor("D338D3");
	public static readonly Color Cyan = GetColor("33BBC8");
	public static readonly Color White = GetColor("DBDBDB");
	public static readonly Color BrightBlack = GetColor("818383");
	public static readonly Color BrightRed = GetColor("FF604A");
	public static readonly Color BrightGreen = GetColor("3BF53B");
	public static readonly Color BrightYellow = GetColor("ECDC23");
	public static readonly Color BrightBlue = GetColor("7490FD");
	public static readonly Color BrightPurple = GetColor("F935F8");
	public static readonly Color BrightCyan = GetColor("14F0F0");
	public static readonly Color BrightWhite = GetColor("FFFFFF");

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
