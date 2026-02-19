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
		Register(new ListParser());
	}

	public void Register(IValueParser parser)
	{
		_parsers[parser.TargetType] = parser;
		_delimiterPairs = null;
		_closeDelimiters = null;
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
	{
		if (_parsers.TryGetValue(targetType, out var parser))
		{
			return parser.HighlightColor;
		}

		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
		{
			return new Color(0.4f, 0.9f, 0.8f);
		}

		if (targetType.IsEnum)
		{
			return new Color(1f, 0.6f, 0.2f);
		}

		return null;
	}

	public string GetHint(Type targetType, string paramName)
	{
		if (_parsers.TryGetValue(targetType, out var parser))
		{
			return paramName + ": " + parser.Hint;
		}

		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
		{
			return paramName + ": [item,item,...]";
		}

		if (targetType.IsEnum)
		{
			return paramName + $": {targetType}";
		}

		return null;
	}

	public List<string> GetSuggestions(Type targetType, string partial)
	{
		string[] candidates = null;

		if (_parsers.TryGetValue(targetType, out var parser))
		{
			candidates = parser.Suggestions;
		}
		else if (targetType.IsEnum)
		{
			candidates = Enum.GetNames(targetType);
		}

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

		if (_parsers.TryGetValue(targetType, out var parser))
		{
			return parser.TryParse(token, out result);
		}

		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
		{
			return TryParseList(token, targetType, out result);
		}

		if (targetType.IsEnum)
		{
			try
			{
				result = Enum.Parse(targetType, token, true);
				return true;
			}
			catch
			{
				return false;
			}
		}

		return false;
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