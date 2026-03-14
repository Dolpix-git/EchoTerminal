using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EchoTerminal
{
public class CommandHighlight
{
	private readonly TerminalHighlightColors _colors;
	private readonly CommandRegistry _registry;

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

		if (!CommandProcessor.TryParseInput(input, out var commandName, out var args, out var leadingSpaces))
		{
			return input;
		}

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

		if (args == null)
		{
			return sb.ToString();
		}

		List<Type[]> overloads = null;
		var hasNonStatic = false;

		if (isKnown)
		{
			overloads = CommandProcessor.GetOverloadParamTypes(entries, out hasNonStatic);
		}

		var pos = leadingSpaces + commandName.Length;
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

			if (CommandProcessor.TryValidateToken(token, expectedType, out var colorType))
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

		var color = colorType != null && _colors.TypeColors.TryGetValue(colorType, out var c)
			? c
			: _colors.FallbackParamColor;

		return $"<color={ToHex(color)}>{token}</color>";
	}

	private static string ToHex(Color c)
	{
		return "#" + ColorUtility.ToHtmlStringRGB(c);
	}
}
}