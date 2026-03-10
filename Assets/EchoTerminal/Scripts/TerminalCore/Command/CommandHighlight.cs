using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EchoTerminal.Scripts.Test;
using UnityEngine;

namespace EchoTerminal
{
public class CommandHighlight
{
	private const string _colorUnknown = "#FF4444";
	private const string _colorCommand = "#AADDFF";
	private const string _colorParam = "#FFDD88";

	private readonly CommandRegistry _registry;

	public CommandHighlight(CommandRegistry registry)
	{
		_registry = registry;
	}

	public string Highlight(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}

		var parsers = CommandProcessor.Parsers;
		var trimmed = input.TrimStart();
		var leadingSpaces = input.Length - trimmed.Length;
		var space = trimmed.IndexOf(' ');
		var commandName = space == -1 ? trimmed : trimmed[..space];
		var isKnown = _registry.TryGet(commandName, out _);
		var cmdColor = isKnown ? _colorCommand : _colorUnknown;

		var sb = new StringBuilder();
		sb.Append(' ', leadingSpaces);
		sb.Append($"<color={cmdColor}>{commandName}</color>");

		if (space == -1)
		{
			return sb.ToString();
		}

		var pos = leadingSpaces + space;
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
			sb.Append(ColorizeToken(token, parsers));
			pos = end;
		}

		return sb.ToString();
	}

	private static string ColorizeToken(string token, IReadOnlyDictionary<Type, IParser> parsers)
	{
		string color;

		if (token.StartsWith("@"))
		{
			color = parsers.TryGetValue(typeof(GameObject), out var p) ? p.HighlightColor : _colorParam;
			return $"<color={color}>{token}</color>";
		}

		if (bool.TryParse(token, out _))
		{
			color = parsers.TryGetValue(typeof(bool), out var p) ? p.HighlightColor : _colorParam;
			return $"<color={color}>{token}</color>";
		}

		if (token.Contains(','))
		{
			color = parsers.TryGetValue(typeof(Vector3), out var p) ? p.HighlightColor : _colorParam;
			return $"<color={color}>{token}</color>";
		}

		if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
		{
			color = parsers.TryGetValue(typeof(float), out var p) ? p.HighlightColor : _colorParam;
			return $"<color={color}>{token}</color>";
		}

		color = parsers.TryGetValue(typeof(string), out var sp) ? sp.HighlightColor : _colorParam;
		return $"<color={color}>{token}</color>";
	}
}
}