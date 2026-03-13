using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace EchoTerminal
{
public class CommandHighlight
{
	private readonly CommandRegistry _registry;
	private readonly TerminalHighlightColors _colors;

	public CommandHighlight(CommandRegistry registry, TerminalHighlightColors colors)
	{
		_registry = registry;
		_colors = colors;
	}

	public string Highlight(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}

		var trimmed = input.TrimStart();
		var leadingSpaces = input.Length - trimmed.Length;
		var space = trimmed.IndexOf(' ');
		var commandName = space == -1 ? trimmed : trimmed[..space];
		var isKnown = _registry.TryGet(commandName, out var entries);

		var sb = new StringBuilder();
		sb.Append(' ', leadingSpaces);

		if (_colors != null)
		{
			var cmdColor = ToHex(isKnown ? _colors.CommandColor : _colors.UnknownColor);
			sb.Append($"<color={cmdColor}>{commandName}</color>");
		}
		else
		{
			sb.Append(commandName);
		}

		if (space == -1)
		{
			return sb.ToString();
		}

		List<Type[]> overloads = null;
		var hasNonStatic = false;

		if (isKnown)
		{
			overloads = BuildOverloadParamTypes(entries, out hasNonStatic);
		}

		var pos = leadingSpaces + space;
		var paramIndex = 0;
		var instanceTargetConsumed = false;

		while (pos < input.Length)
		{
			if (input[pos] == ' ')
			{
				sb.Append(' ');
				pos++;
				continue;
			}

			var end = input.IndexOf(' ', pos);
			if (end == -1)
			{
				end = input.Length;
			}

			var token = input[pos..end];

			if (hasNonStatic && !instanceTargetConsumed && token.StartsWith("@"))
			{
				instanceTargetConsumed = true;
				sb.Append(ColorizeTyped(token, typeof(GameObject)));
			}
			else
			{
				sb.Append(ColorizeAtPosition(token, overloads, paramIndex));
				paramIndex++;
			}

			pos = end;
		}

		return sb.ToString();
	}

	private string ColorizeAtPosition(string token, List<Type[]> overloads, int paramIndex)
	{
		if (_colors == null)
		{
			return token;
		}

		if (overloads == null)
		{
			return $"<color={ToHex(_colors.FallbackParamColor)}>{token}</color>";
		}

		foreach (var overload in overloads)
		{
			if (paramIndex >= overload.Length)
			{
				continue;
			}

			var expectedType = overload[paramIndex];

			if (TryValidateToken(token, expectedType, out var colorType))
			{
				return ColorizeTyped(token, colorType);
			}
		}

		return $"<color={ToHex(_colors.UnknownColor)}>{token}</color>";
	}

	private string ColorizeTyped(string token, Type colorType)
	{
		if (_colors == null)
		{
			return token;
		}

		var color = (colorType != null && _colors.TypeColors.TryGetValue(colorType, out var c))
			? c
			: _colors.FallbackParamColor;

		return $"<color={ToHex(color)}>{token}</color>";
	}

	private static bool TryValidateToken(string token, Type expectedType, out Type colorType)
	{
		colorType = null;
		var parsers = CommandProcessor.Parsers;

		if (expectedType == typeof(Terminal))
		{
			return true;
		}

		if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(List<>))
		{
			var elementType = expectedType.GetGenericArguments()[0];
			colorType = elementType;

			foreach (var part in token.Split(','))
			{
				var trimmed = part.Trim();

				if (parsers.TryGetValue(elementType, out var ep))
				{
					if (!ep.TryParse(trimmed, out _, out _))
					{
						return false;
					}
				}
				else if (elementType.IsEnum)
				{
					if (!Enum.TryParse(elementType, trimmed, true, out _))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		if (parsers.TryGetValue(expectedType, out var parser))
		{
			colorType = expectedType;
			return parser.TryParse(token, out _, out _);
		}

		if (expectedType.IsEnum)
		{
			colorType = expectedType;
			return Enum.TryParse(expectedType, token, true, out _);
		}

		return false;
	}

	private static List<Type[]> BuildOverloadParamTypes(List<CommandEntry> entries, out bool hasNonStatic)
	{
		hasNonStatic = false;
		var result = new List<Type[]>();

		foreach (var entry in entries)
		{
			if (!entry.IsStatic)
			{
				hasNonStatic = true;
			}

			var paramInfos = entry.Method.GetParameters();
			var types = new List<Type>();

			foreach (var p in paramInfos)
			{
				if (p.ParameterType == typeof(Terminal))
				{
					continue;
				}

				types.Add(p.ParameterType);
			}

			result.Add(types.ToArray());
		}

		return result;
	}

	private static string ToHex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
}
}