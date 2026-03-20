using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EchoTerminal
{
public class CommandHighlight
{
	private readonly TerminalHighlightColors _colors;
	private readonly CommandParser _parser;

	public CommandHighlight(CommandParser parser, TerminalHighlightColors colors)
	{
		_parser = parser;
		_colors = colors;
	}

	public string Highlight(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}

		var result = _parser.Parse(input);

		if (string.IsNullOrEmpty(result.CommandName))
		{
			return input;
		}

		var sb = new StringBuilder();
		sb.Append(' ', result.LeadingSpaces);

		if (_colors != null)
		{
			var cmdColor = ToHex(result.IsKnownCommand || IsPartialCommandName(result.CommandName)
				? _colors.CommandColor
				: _colors.UnknownColor);

			sb.Append($"<color={cmdColor}>{result.CommandName}</color>");
		}
		else
		{
			sb.Append(result.CommandName);
		}

		if (result.Args == null || result.Overloads.Count == 0)
		{
			return sb.ToString();
		}

		sb.Append(' ');

		var overload = PickBestOverload(result.Overloads);
		var args = result.Args;
		var pos = 0;

		var stillTyping = input.Length > 0 && input[^1] != ' ';

		foreach (var param in overload.Params)
		{
			if (param.Token == null)
			{
				continue;
			}

			var tokenIdx = args.IndexOf(param.Token, pos, StringComparison.Ordinal);
			if (tokenIdx < 0)
			{
				break;
			}

			sb.Append(args, pos, tokenIdx - pos);

			var isLastToken = stillTyping && tokenIdx + param.Token.Length == args.Length;

			sb.Append(_colors != null
				? param.IsValid
					? ColorizeTyped(param.Token, param.Expected.Type)
					: isLastToken
						? $"<color={ToHex(_colors.FallbackParamColor)}>{EscapeForTmp(param.Token)}</color>"
						: $"<color={ToHex(_colors.UnknownColor)}>{EscapeForTmp(param.Token)}</color>"
				: param.Token);

			pos = tokenIdx + param.Token.Length;
		}

		if (pos < args.Length)
		{
			var trailing = args[pos..];
			var content = trailing.TrimStart();
			var wsLen = trailing.Length - content.Length;

			sb.Append(trailing, 0, wsLen);

			if (content.Length > 0)
			{
				sb.Append(_colors != null
					? stillTyping
						? $"<color={ToHex(_colors.FallbackParamColor)}>{EscapeForTmp(content)}</color>"
						: $"<color={ToHex(_colors.UnknownColor)}>{EscapeForTmp(content)}</color>"
					: content);
			}
		}

		return sb.ToString();
	}

	private bool IsPartialCommandName(string name)
	{
		foreach (var cmd in _parser.Registry.GetCommandNames())
		{
			if (cmd.StartsWith(name, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private static OverloadResult PickBestOverload(IReadOnlyList<OverloadResult> overloads)
	{
		foreach (var o in overloads)
		{
			if (o.IsComplete)
			{
				return o;
			}
		}

		var best = overloads[0];
		var bestValid = 0;

		foreach (var o in overloads)
		{
			var count = 0;
			foreach (var p in o.Params)
			{
				if (p.IsValid)
				{
					count++;
				}
			}

			if (count > bestValid)
			{
				best = o;
				bestValid = count;
			}
		}

		return best;
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

		return $"<color={ToHex(color)}>{EscapeForTmp(token)}</color>";
	}

	private static string EscapeForTmp(string s)
	{
		return s.Contains('<') ? s.Replace("<", "<noparse><</noparse>") : s;
	}

	private static string ToHex(Color c)
	{
		return "#" + ColorUtility.ToHtmlStringRGB(c);
	}
}
}