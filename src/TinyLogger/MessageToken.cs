using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TinyLogger;

public abstract partial record MessageToken
{
	public abstract void Write(StringBuilder sb);

#if NET7_0_OR_GREATER
	[GeneratedRegex(@"^{(?<value>[^,:]+)(,(?<alignment>[\d\-]+))?(:(?<format>[^}]+))?}$", RegexOptions.ExplicitCapture)]
	private static partial Regex GetFormatMatcher();
	private static readonly Regex formatMatcher = GetFormatMatcher();
#else
	private static readonly Regex formatMatcher = new(@"^{(?<value>[^,:]+)(,(?<alignment>[\d\-]+))?(:(?<format>[^}]+))?}$", RegexOptions.ExplicitCapture);
#endif

	/// <summary>
	/// Create a token from a format string, examples:
	/// "{name}"
	/// "{name,2}"
	/// "{name:#.##}"
	/// "{name,5:#.##}"
	/// </summary>
	/// <param name="value">The value to be parsed</param>
	public static MessageToken FromFormat(string value)
	{
#if NET
		ArgumentNullException.ThrowIfNull(value);
#else
		if (value is null)
			throw new ArgumentNullException(nameof(value));
#endif

		// Avoid calling the Regex if possible to reduce allocations

		if (value[0] != '{' || value[^1] != '}')
		{
			return new LiteralToken(value);
		}

#if NET
		if (value.Contains(':', StringComparison.Ordinal) || value.Contains(',', StringComparison.Ordinal))
#else
		if (value.Contains(':') || value.Contains(','))
#endif
		{
			var match = formatMatcher.Match(value);

			if (match.Success)
			{
				var valueStr = match.Groups["value"].Value;
				var alignmentStr = match.Groups["alignment"].Value;
				var formatStr = match.Groups["format"].Value;

				return new ObjectToken(
					valueStr,
					Alignment: int.TryParse(alignmentStr, out var alignmentValue) ? alignmentValue : (int?)null,
					Format: !string.IsNullOrEmpty(formatStr) ? formatStr : null
				);
			}
		}
		else
		{
			return new ObjectToken(value[1..^1]);
		}

		return new LiteralToken(value);
	}
}

public record LiteralToken(string Value) :
	MessageToken
{
	public override string ToString()
	{
		return Value;
	}

	public override void Write(StringBuilder sb)
	{
#if NET
		ArgumentNullException.ThrowIfNull(sb);
#else
		if (sb is null)
			throw new ArgumentNullException(nameof(sb));
#endif

		sb.Append(Value);
	}
}

public record ObjectToken(object? Value, int? Alignment = null, string? Format = null) :
	MessageToken
{
	protected string FormatString => $"{{0{(Alignment != null ? "," + Alignment : null)}{(Format != null ? ":" + Format : null)}}}";

	public override string ToString()
	{
		if (Value is string str)
		{
			return str;
		}

		return string.Format(CultureInfo.CurrentCulture, FormatString, Value);
	}

	public override void Write(StringBuilder sb)
	{
#if NET
		ArgumentNullException.ThrowIfNull(sb);
#else
		if (sb is null)
			throw new ArgumentNullException(nameof(sb));
#endif

		if (Value is string str)
		{
			sb.Append(str);
		}
		else
		{
			sb.AppendFormat(CultureInfo.CurrentCulture, FormatString, Value);
		}
	}
}

public record ObjectTokenWithTransform<T> : ObjectToken
{
	public Func<T, object> ValueTransform { get; init; }

	public ObjectTokenWithTransform(T value, Func<T, object> valueTransform, int? alignment = null, string? format = null)
		: base(value, alignment, format)
	{
		ValueTransform = valueTransform;
	}

	public override string ToString()
	{
		var transformedValue = Value != null ? ValueTransform((T)Value) : Value;

		if (transformedValue is string str)
		{
			return str;
		}

		return string.Format(CultureInfo.CurrentCulture, FormatString, transformedValue);
	}

	public override void Write(StringBuilder sb)
	{
#if NET
		ArgumentNullException.ThrowIfNull(sb);
#else
		if (sb is null)
			throw new ArgumentNullException(nameof(sb));
#endif

		var transformedValue = Value != null ? ValueTransform((T)Value) : Value;

		if (transformedValue is string str)
		{
			sb.Append(str);
		}
		else
		{
			sb.AppendFormat(CultureInfo.CurrentCulture, FormatString, transformedValue);
		}
	}
}
