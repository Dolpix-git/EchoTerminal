using System.Collections.Generic;
using System.Text;

namespace EchoTerminal
{
public class HintAnalyzer
{
	private readonly CommandParser _parser;

	public HintAnalyzer(CommandParser parser)
	{
		_parser = parser;
	}

	public List<string> GetHints(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return null;
		}

		var analysis = _parser.Analyze(input);

		if (analysis.Spans.Count == 0)
		{
			return null;
		}

		if (!analysis.CommandValid)
		{
			return null;
		}

		if (analysis.SubCommandOffset >= 0
			&& analysis.EditingIndex >= analysis.SubCommandOffset
			&& analysis.SubCommandOffset < analysis.Spans.Count)
		{
			var subInputStart = analysis.Spans[analysis.SubCommandOffset].start;
			var subInput = input.Substring(subInputStart);
			return GetHints(subInput);
		}

		var matches = CommandRegistry.Instance.FindCommands(analysis.CommandName, null);

		if (matches.Count == 0)
		{
			return null;
		}

		var typedArgs = new List<string>();

		for (var i = analysis.ArgStart; i < analysis.Spans.Count; i++)
		{
			typedArgs.Add(analysis.Spans[i].token);
		}

		var completedArgCount = typedArgs.Count;

		if (completedArgCount > 0 && !analysis.TrailingSpace)
		{
			completedArgCount--;
		}

		var showPlain = !analysis.HasTarget;
		var showTarget = analysis.HasTarget || analysis.Spans.Count <= 1;

		var hints = new List<string>();
		var seen = new HashSet<string>();

		foreach (var (target, method) in matches)
		{
			var parameters = method.GetParameters();
			var isStatic = target == null;

			if (completedArgCount > parameters.Length)
			{
				continue;
			}

			var compatible = true;

			for (var i = 0; i < completedArgCount; i++)
			{
				if (_parser.Values.TryConvertSingle(typedArgs[i], parameters[i].ParameterType, out _))
				{
					continue;
				}

				compatible = false;
				break;
			}

			if (!compatible)
			{
				continue;
			}

			var sb = new StringBuilder();

			if (showPlain)
			{
				sb.Append(analysis.CommandName);

				foreach (var param in parameters)
				{
					sb.Append(' ');
					sb.Append(_parser.Values.GetHint(param.ParameterType, param.Name));
				}

				var sig = sb.ToString();

				if (seen.Add(sig))
				{
					hints.Add(sig);
				}
			}

			if (!showTarget || isStatic)
			{
				continue;
			}

			sb.Clear();
			sb.Append(analysis.CommandName).Append(" @target");

			foreach (var param in parameters)
			{
				sb.Append(' ');
				sb.Append(_parser.Values.GetHint(param.ParameterType, param.Name));
			}

			var sigTarget = sb.ToString();

			if (seen.Add(sigTarget))
			{
				hints.Add(sigTarget);
			}
		}

		return hints.Count > 0 ? hints : null;
	}
}
}
