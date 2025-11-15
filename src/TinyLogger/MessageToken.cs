using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TinyLogger;

public abstract partial record MessageToken
{
	public abstract bool TryGetValue([NotNullWhen(true)] out object? value);

	public abstract bool TryGetValue<T>([NotNullWhen(true)] out T value);

	public MessageToken WithFormatFrom(ObjectToken? token)
	{
		if (token is null)
		{
			return this;
		}

		var hasFormat = token.Alignment != null || token.Format != null;

		return hasFormat && this is ObjectToken ot ? ot with { Alignment = token.Alignment, Format = token.Format } : this;
	}

	public abstract void Write(StringBuilder sb);

#if NET
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
		ArgumentNullException.ThrowIfNull(value);

		// Avoid calling the Regex if possible to reduce allocations

		if (value[0] != '{' || value[^1] != '}')
		{
			return new LiteralToken(value);
		}

		if (value.Contains(':', StringComparison.Ordinal) || value.Contains(',', StringComparison.Ordinal))
		{
			var match = formatMatcher.Match(value);

			if (match.Success)
			{
				var valueStr = match.Groups["value"].Value;
				var alignmentStr = match.Groups["alignment"].Value;
				var formatStr = match.Groups["format"].Value;

				return new ObjectToken<string>(
					valueStr,
					Alignment: int.TryParse(alignmentStr, out var alignmentValue) ? alignmentValue : (int?)null,
					Format: !string.IsNullOrEmpty(formatStr) ? formatStr : null
				);
			}
		}
		else
		{
			return new ObjectToken<string>(value[1..^1]);
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

	public override bool TryGetValue([NotNullWhen(true)] out object? value)
	{
		value = Value;
		return true;
	}

	public override bool TryGetValue<T>([NotNullWhen(true)] out T value)
	{
		if (Value is T t)
		{
			value = t;
			return true;
		}

		value = default!;
		return false;
	}

	public override void Write(StringBuilder sb)
	{
		ArgumentNullException.ThrowIfNull(sb);

		sb.Append(Value);
	}
}

public abstract record ObjectToken(int? Alignment = null, string? Format = null) :
	MessageToken;

public record ObjectToken<T>(T? Value, int? Alignment = null, string? Format = null) :
	ObjectToken(Alignment, Format)
{
	private object? objectValue;

	protected string FormatString => $"{{0{(Alignment != null ? "," + Alignment : null)}{(Format != null ? ":" + Format : null)}}}";

	public override string ToString()
	{
		if (Value is string str)
		{
			return str;
		}
		else if (Alignment is null && Format is null)
		{
			return Value?.ToString() ?? string.Empty;
		}

		return string.Format(CultureInfo.CurrentCulture, FormatString, Value);
	}

	public override bool TryGetValue([NotNullWhen(true)] out object? value)
	{
		value = objectValue ??= Value;
		return value != null;
	}

	public override bool TryGetValue<TValue>([NotNullWhen(true)] out TValue value)
	{
		if (Value is TValue val)
		{
			value = val;
			return true;
		}

		value = default!;
		return false;
	}

	public override void Write(StringBuilder sb)
	{
		ArgumentNullException.ThrowIfNull(sb);

		if (Value is string || Alignment is null && Format is null)
		{
			sb.Append(Value);
		}
		else
		{
			sb.AppendFormat(CultureInfo.CurrentCulture, FormatString, Value);
		}
	}
}

public record ObjectTokenWithTransform<T> : ObjectToken<T>
{
	public Func<T, object> ValueTransform { get; init; }

	public ObjectTokenWithTransform(T value, Func<T, object> valueTransform, int? alignment = null, string? format = null)
		: base(value, alignment, format)
	{
		ValueTransform = valueTransform;
	}

	public override string ToString()
	{
		var transformedValue = Value != null ? ValueTransform(Value) : Value;

		if (transformedValue is string str)
		{
			return str;
		}

		return string.Format(CultureInfo.CurrentCulture, FormatString, transformedValue);
	}

	public override void Write(StringBuilder sb)
	{
		ArgumentNullException.ThrowIfNull(sb);

		var transformedValue = Value != null ? ValueTransform(Value) : Value;

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

public record FuncToken(Func<MessageToken> GetToken) : MessageToken
{
	public override string ToString()
	{
		return GetToken().ToString();
	}

	public override bool TryGetValue([NotNullWhen(true)] out object? value)
	{
		return GetToken().TryGetValue(out value);
	}

	public override bool TryGetValue<T>([NotNullWhen(true)] out T value)
	{
		return GetToken().TryGetValue<T>(out value);
	}

	public override void Write(StringBuilder sb)
	{
		GetToken().Write(sb);
	}
}

public record TokenTemplate(IReadOnlyList<MessageToken> MessageTokens) : MessageToken
{
	public override string ToString()
	{
		using var sb = Pooling.RentStringBuilder();

		Write(sb.Value);

		return sb.Value.ToString();
	}

	public override bool TryGetValue([NotNullWhen(true)] out object? value)
	{
		value = null;
		return false;
	}

	public override bool TryGetValue<T>([NotNullWhen(true)] out T value)
	{
		value = default!;
		return false;
	}

	public override void Write(StringBuilder sb)
	{
		foreach (var token in MessageTokens)
		{
			token.Write(sb);
		}
	}
}
