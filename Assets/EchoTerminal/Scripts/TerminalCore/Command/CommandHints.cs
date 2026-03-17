using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoTerminal
{
public class CommandHints
{
	private readonly TerminalHighlightColors _colors;
	private readonly CommandParser _parser;

	public CommandHints(CommandParser parser, TerminalHighlightColors colors)
	{
		_parser = parser;
		_colors = colors;
	}

	public List<string> GetHints(string input)
	{
		var result = _parser.Parse(input);

		if (!result.IsKnownCommand || result.Args == null)
		{
			return null;
		}

		var hints = new List<string>();
		foreach (var overload in result.Overloads)
		{
			var parts = new List<string>();
			var activeFound = false;

			foreach (var param in overload.Params)
			{
				var str = param.Expected.ToString();
				if (!activeFound && !param.IsValid)
				{
					str = $"<b>{Colorize(str, param.Expected.Type)}</b>";
					activeFound = true;
				}
				parts.Add(str);
			}

			if (parts.Count > 0)
			{
				hints.Add(string.Join(" ", parts));
			}
		}

		return hints.Count > 0 ? hints : null;
	}

	private string Colorize(string text, Type type)
	{
		if (_colors == null)
		{
			return text;
		}

		var color = _colors.TypeColors.TryGetValue(type, out var c) ? c : _colors.FallbackParamColor;
		return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
	}
}
}