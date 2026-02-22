using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EchoTerminal
{
public class ValueParserRegistry
{
	private readonly Dictionary<Type, IValueParser> _parsers = new();
	private readonly Dictionary<Type, IValueParser> _enumParsers = new();
	private readonly IValueParser _listParser;
	private Dictionary<char, char> _delimiterPairs;
	private HashSet<char> _closeDelimiters;

	public ValueParserRegistry()
	{
		Register(new StringParser());
		Register(new IntParser());
		Register(new FloatParser());
		Register(new DoubleParser());
		Register(new BoolParser());
		Register(new Vector2Parser());
		Register(new Vector3Parser());
		Register(new ColorParser());
		_listParser = new ListParser();
		Register(_listParser);
	}

	public void Register(IValueParser parser)
	{
		_parsers[parser.TargetType] = parser;
		_delimiterPairs = null;
		_closeDelimiters = null;
	}

	// Resolves a parser for any type: registered, List<T>, or Enum.
	// Enum parsers are created on demand and cached separately to avoid
	// invalidating the delimiter cache.
	private IValueParser FindParser(Type targetType)
	{
		if (_parsers.TryGetValue(targetType, out var parser))
		{
			return parser;
		}

		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
		{
			return _listParser;
		}

		if (targetType.IsEnum)
		{
			if (!_enumParsers.TryGetValue(targetType, out var enumParser))
			{
				enumParser = new EnumParser(targetType);
				_enumParsers[targetType] = enumParser;
			}

			return enumParser;
		}

		return null;
	}

	public Dictionary<char, char> GetDelimiterPairs()
	{
		if (_delimiterPairs != null)
		{
			return _delimiterPairs;
		}

		_delimiterPairs = new();

		foreach (var parser in _parsers.Values)
		{
			if (parser.OpenDelimiter.HasValue && parser.CloseDelimiter.HasValue)
			{
				_delimiterPairs[parser.OpenDelimiter.Value] = parser.CloseDelimiter.Value;
			}
		}

		return _delimiterPairs;
	}

	public Color? GetHighlightColor(Type targetType)
		=> FindParser(targetType)?.HighlightColor;

	public string GetHint(Type targetType, string paramName)
	{
		var parser = FindParser(targetType);
		return parser != null ? paramName + ": " + parser.Hint : null;
	}

	public List<string> GetSuggestions(Type targetType, string partial)
	{
		var candidates = FindParser(targetType)?.Suggestions;

		if (candidates == null || candidates.Length == 0)
		{
			return null;
		}

		var filtered = new List<string>();

		foreach (var candidate in candidates)
		{
			if (string.IsNullOrEmpty(partial) ||
				candidate.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
			{
				filtered.Add(candidate);
			}
		}

		return filtered.Count > 0 ? filtered : null;
	}

	
	
	public bool TryConvertSingle(string token, Type targetType, out object result)
	{
		result = null;
		var parser = FindParser(targetType);

		if (parser == null)
		{
			return false;
		}

		if (parser is ListParser)
		{
			return TryParseList(token, targetType, out result);
		}

		return parser.TryParse(token, out result);
	}

	private bool TryParseList(string token, Type listType, out object result)
	{
		result = null;
		var elementType = listType.GetGenericArguments()[0];

		if (token.Length < 2 || token[0] != '[' || token[^1] != ']')
		{
			return false;
		}

		var inner = token.Substring(1, token.Length - 2);

		if (string.IsNullOrWhiteSpace(inner))
		{
			result = Activator.CreateInstance(listType);
			return true;
		}

		var elements = SplitElements(inner);
		var list = (IList)Activator.CreateInstance(listType);

		foreach (var element in elements)
		{
			if (!TryConvertSingle(element, elementType, out var parsed))
			{
				return false;
			}

			list.Add(parsed);
		}

		result = list;
		return true;
	}

	private List<string> SplitElements(string inner)
	{
		var pairs = GetDelimiterPairs();
		var closes = GetCloseDelimiters();
		var parts = new List<string>();
		var current = new StringBuilder();
		var depth = 0;

		for (var i = 0; i < inner.Length; i++)
		{
			var ch = inner[i];

			if (depth == 0 && pairs != null && pairs.TryGetValue(ch, out var close))
			{
				if (ch == close)
				{
					current.Append(ch);
					i++;

					while (i < inner.Length && inner[i] != close)
					{
						current.Append(inner[i]);
						i++;
					}

					if (i < inner.Length)
					{
						current.Append(inner[i]);
					}

					continue;
				}

				depth++;
				current.Append(ch);
				continue;
			}

			if (depth > 0 && closes.Contains(ch))
			{
				depth--;
				current.Append(ch);
				continue;
			}

			if (ch == ',' && depth == 0)
			{
				parts.Add(current.ToString().Trim());
				current.Clear();
				continue;
			}

			current.Append(ch);
		}

		if (current.Length > 0)
		{
			parts.Add(current.ToString().Trim());
		}

		return parts;
	}

	private HashSet<char> GetCloseDelimiters()
	{
		if (_closeDelimiters != null)
		{
			return _closeDelimiters;
		}

		_closeDelimiters = new();
		var pairs = GetDelimiterPairs();

		foreach (var (open, close) in pairs)
		{
			if (open != close)
			{
				_closeDelimiters.Add(close);
			}
		}

		return _closeDelimiters;
	}
}
}
