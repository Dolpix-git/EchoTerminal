using System.Text;
using UnityEngine;

namespace EchoTerminal
{
public class CommandHighlighter
{
	private static readonly Color ErrorColor = new(1f, 0.3f, 0.3f);
	private static readonly Color IncompleteColor = new(0.6f, 0.6f, 0.6f);
	private readonly CommandParser _parser;

	public CommandHighlighter(CommandParser parser)
	{
		_parser = parser;
	}

	public string Highlight(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "";
		}

		var analysis = _parser.Analyze(input);

		if (analysis.Spans.Count == 0)
		{
			return "";
		}

		var sb = new StringBuilder();
		var lastEnd = 0;

		for (var i = 0; i < analysis.Spans.Count; i++)
		{
			if (analysis.SubCommandOffset >= 0 && i == analysis.SubCommandOffset)
			{
				var subInputStart = analysis.Spans[i].start;

				if (subInputStart > lastEnd)
				{
					sb.Append(input, lastEnd, subInputStart - lastEnd);
				}

				var subInput = input.Substring(subInputStart);
				sb.Append(Highlight(subInput));
				lastEnd = input.Length;
				break;
			}

			var (token, start, end) = analysis.Spans[i];
			var isIncomplete = i == analysis.Spans.Count - 1 && !analysis.TrailingSpace;
			var fallback = isIncomplete ? IncompleteColor : ErrorColor;

			Color color;

			if (i == 0)
			{
				color = analysis.CommandValid ? _parser.CommandNameColor : fallback;
			}
			else if (i < analysis.ArgStart)
			{
				color = _parser.TargetColor;
			}
			else
			{
				var argIndex = i - analysis.ArgStart;

				if (analysis.Parameters != null && argIndex < analysis.Parameters.Length)
				{
					var paramType = analysis.Parameters[argIndex].ParameterType;
					color = _parser.Values.TryConvertSingle(token, paramType, out _)
						? _parser.Values.GetHighlightColor(paramType) ?? fallback
						: fallback;
				}
				else
				{
					color = fallback;
				}
			}

			if (start > lastEnd)
			{
				sb.Append(input, lastEnd, start - lastEnd);
			}

			sb.Append("<color=#");
			sb.Append(ColorUtility.ToHtmlStringRGB(color));
			sb.Append('>');
			sb.Append(input, start, end - start);
			sb.Append("</color>");

			lastEnd = end;
		}

		if (lastEnd < input.Length)
		{
			sb.Append(input, lastEnd, input.Length - lastEnd);
		}

		return sb.ToString();
	}
}
}
