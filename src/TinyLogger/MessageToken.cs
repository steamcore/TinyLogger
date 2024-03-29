using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TinyLogger;

public enum MessageTokenType
{
	/// <summary>
	/// Literal tokens should be rendered as plain text
	/// </summary>
	LiteralToken,

	/// <summary>
	/// Object tokens can be tokenized and rendered using color
	/// </summary>
	ObjectToken
}

public readonly partial struct MessageToken(
	object? value,
	MessageTokenType type,
	int? alignment = null,
	string? format = null,
	Func<object, object>? valueTransform = null
)
	: IEquatable<MessageToken>
{
	/// <summary>
	/// Alignment from format string
	/// </summary>
	public int? Alignment { get; } = alignment;

	/// <summary>
	/// Format from format string
	/// </summary>
	public string? Format { get; } = format;

	/// <summary>
	/// The actual value represented by this token
	/// </summary>
	public object? Value { get; } = value;

	/// <summary>
	/// Transform the value when it is to be rendered, for example LogLevel.Information -> "info"
	/// </summary>
	public Func<object, object>? ValueTransform { get; } = valueTransform;

	/// <summary>
	/// Token type
	/// </summary>
	public MessageTokenType Type { get; } = type;

	public override string ToString()
	{
		var transformedValue = Value != null && ValueTransform != null ? ValueTransform(Value) : Value;

		if (transformedValue is string str)
		{
			return str;
		}

		return string.Format(CultureInfo.CurrentCulture, GetFormatString(), transformedValue);
	}

	public void Write(StringBuilder sb)
	{
#if NET
		ArgumentNullException.ThrowIfNull(sb);
#else
		if (sb is null)
			throw new ArgumentNullException(nameof(sb));
#endif

		var transformedValue = Value != null && ValueTransform != null ? ValueTransform(Value) : Value;

		if (transformedValue is string str)
		{
			sb.Append(str);
		}
		else
		{
			sb.AppendFormat(CultureInfo.CurrentCulture, GetFormatString(), transformedValue);
		}
	}

	public MessageToken WithFormat(MessageToken formatToken)
	{
		return new MessageToken(
			Value,
			Type,
			Alignment ?? formatToken.Alignment,
			Format ?? formatToken.Format,
			ValueTransform
		);
	}

	public bool Equals(MessageToken other)
	{
		if (Type != other.Type)
			return false;

		return (Value is null && other.Value is null || Value?.Equals(other.Value) == true)
			&& (Alignment is null && other.Alignment is null || Alignment?.Equals(other.Alignment) == true)
			&& (Format is null && other.Format is null || Format?.Equals(other.Format, StringComparison.Ordinal) == true);
	}

	public override bool Equals(object? obj)
	{
		if (obj is not MessageToken other)
			return false;

		return Equals(other);
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}

	private string GetFormatString()
	{
		return $"{{0{(Alignment != null ? "," + Alignment : null)}{(Format != null ? ":" + Format : null)}}}";
	}

	public static bool operator ==(MessageToken left, MessageToken right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(MessageToken left, MessageToken right)
	{
		return !(left == right);
	}

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

#if NET6_0_OR_GREATER
		if (value[0] != '{' || value[^1] != '}')
#else
		if (value[0] != '{' || value[value.Length - 1] != '}')
#endif
		{
			return new MessageToken(value, MessageTokenType.LiteralToken);
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

				return new MessageToken(
					valueStr,
					MessageTokenType.ObjectToken,
					alignment: int.TryParse(alignmentStr, out var alignmentValue) ? alignmentValue : (int?)null,
					format: !string.IsNullOrEmpty(formatStr) ? formatStr : null
				);
			}
		}
		else
		{
			return new MessageToken(
#if NET6_0_OR_GREATER
				value[1..^1],
#else
				value.Substring(1, value.Length - 2),
#endif
				MessageTokenType.ObjectToken,
				null,
				null
			);
		}

		return new MessageToken(value, MessageTokenType.LiteralToken);
	}

	/// <summary>
	/// Create a literal token that will be rendered as plain text
	/// </summary>
	public static MessageToken FromLiteral(object? value, int? alignment = null, string? format = null, Func<object, object>? valueTransform = null)
	{
		return new MessageToken(value, MessageTokenType.LiteralToken, alignment, format, valueTransform);
	}

	/// <summary>
	/// Create an object token that can be rendered in color
	/// </summary>
	public static MessageToken FromObject(object? value, int? alignment = null, string? format = null, Func<object, object>? valueTransform = null)
	{
		return new MessageToken(value, MessageTokenType.ObjectToken, alignment, format, valueTransform);
	}
}
