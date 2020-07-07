using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TinyLogger
{
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

	public class MessageToken : IEquatable<MessageToken>
	{
		/// <summary>
		/// Alignment from format string
		/// </summary>
		public int? Alignment { get; }

		/// <summary>
		/// Format from format string
		/// </summary>
		public string? Format { get; }

		/// <summary>
		/// The actual value represented by this token
		/// </summary>
		public object? Value { get; }

		/// <summary>
		/// Transform the value when it is to be rendered, for example LogLevel.Information -> "info"
		/// </summary>
		public Func<object, object>? ValueTransform { get; }

		/// <summary>
		/// Token type
		/// </summary>
		public MessageTokenType Type { get; }

		public MessageToken(object? value, MessageTokenType type, int? alignment = null, string? format = null, Func<object, object>? valueTransform= null)
		{
			Value = value;
			Type = type;

			Alignment = alignment;
			Format = format;
			ValueTransform = valueTransform;
		}

		public string GetFormatString()
		{
			return $"{{0{(Alignment != null ? "," + Alignment : null)}{(Format != null ? ":" + Format : null)}}}";
		}

		public override string ToString()
		{
			var value = Value != null && ValueTransform != null ? ValueTransform(Value) : Value;

			if (value is string str)
			{
				return str;
			}

			return string.Format(CultureInfo.CurrentCulture, GetFormatString(), value);
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

		public bool Equals(MessageToken? other)
		{
			if (Type != other?.Type)
				return false;

			return (Value == null && other.Value == null || Value?.Equals(other.Value) == true)
				&& (Alignment == null && other.Alignment == null || Alignment?.Equals(other.Alignment) == true)
				&& (Format == null && other.Format == null || Format?.Equals(other.Format, StringComparison.Ordinal) == true);
		}

		public override bool Equals(object? obj)
		{
			if (!(obj is MessageToken other))
				return false;

			return Equals(other);
		}

		public override int GetHashCode()
		{
			return Value?.GetHashCode() ?? 0;
		}

		public static bool operator ==(MessageToken left, MessageToken right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MessageToken left, MessageToken right)
		{
			return !(left == right);
		}

		private static readonly Regex formatMatcher = new Regex(@"^{(?<value>[^,:]+)(,(?<alignment>[\d\-]+))?(:(?<format>[^}]+))?}$", RegexOptions.ExplicitCapture);

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
			var match = formatMatcher.Match(value);

			if (match.Success)
			{
				var valueStr = match.Groups["value"].Value;
				var alignmentStr = match.Groups["alignment"].Value;
				var formatStr = match.Groups["format"].Value;

				return new MessageToken(
					valueStr,
					MessageTokenType.ObjectToken,
					alignment: int.TryParse(alignmentStr, out var alignment) ? alignment : (int?)null,
					format: !string.IsNullOrEmpty(formatStr) ? formatStr : null
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
}
