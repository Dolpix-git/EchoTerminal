using System;
using System.Collections.Generic;

namespace EchoTerminal
{
public class SuggestionAnalyzer
{
	private readonly CommandParser _parser;

	public SuggestionAnalyzer(CommandParser parser)
	{
		_parser = parser;
	}

	public AutocompleteContext GetSuggestions(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return AutocompleteContext.Empty;
		}

		var analysis = _parser.Analyze(input);

		if (analysis.Spans.Count == 0)
		{
			return AutocompleteContext.Empty;
		}

		if (analysis.SubCommandOffset >= 0
			&& analysis.EditingIndex >= analysis.SubCommandOffset)
		{
			return GetSubCommandSuggestions(input, analysis);
		}

		if (analysis.EditingIndex == 0)
		{
			return GetCommandNameSuggestions(analysis.Partial, analysis.ReplaceStart, analysis.ReplaceEnd);
		}

		if (!analysis.CommandValid)
		{
			return AutocompleteContext.Empty;
		}

		var isTargetPosition = analysis.EditingIndex == 1
			&& (analysis.HasTarget || analysis.Partial.StartsWith("@"));

		if (isTargetPosition)
		{
			var targetPartial = analysis.Partial.Length > 1 ? analysis.Partial.Substring(1) : "";
			var targetNames = CommandRegistry.Instance.GetTargetNames(analysis.CommandName);
			var filtered = new List<string>();

			foreach (var name in targetNames)
			{
				if (string.IsNullOrEmpty(targetPartial) ||
					name.StartsWith(targetPartial, StringComparison.OrdinalIgnoreCase))
				{
					filtered.Add("@" + name);
				}
			}

			filtered.Sort(StringComparer.OrdinalIgnoreCase);

			return new()
			{
				Suggestions = filtered.Count > 0 ? filtered : null,
				ReplaceStart = analysis.ReplaceStart,
				ReplaceEnd = analysis.ReplaceEnd
			};
		}

		var argIndex = analysis.EditingIndex - analysis.ArgStart;

		if (analysis.Parameters == null || argIndex < 0 || argIndex >= analysis.Parameters.Length)
		{
			return AutocompleteContext.Empty;
		}

		var param = analysis.Parameters[argIndex];
		var suggestions = _parser.Values.GetSuggestions(param.ParameterType, analysis.Partial);

		return new()
		{
			Suggestions = suggestions,
			ReplaceStart = analysis.ReplaceStart,
			ReplaceEnd = analysis.ReplaceEnd
		};
	}

	private AutocompleteContext GetSubCommandSuggestions(string input, InputAnalysis analysis)
	{
		if (analysis.SubCommandOffset < analysis.Spans.Count)
		{
			var subInputStart = analysis.Spans[analysis.SubCommandOffset].start;
			var subInput = input.Substring(subInputStart);
			var subResult = GetSuggestions(subInput);

			if (subResult == AutocompleteContext.Empty)
			{
				return AutocompleteContext.Empty;
			}

			return new()
			{
				Suggestions = subResult.Suggestions,
				ReplaceStart = subResult.ReplaceStart + subInputStart,
				ReplaceEnd = subResult.ReplaceEnd + subInputStart
			};
		}

		return GetCommandNameSuggestions("", analysis.ReplaceStart, analysis.ReplaceEnd);
	}

	private AutocompleteContext GetCommandNameSuggestions(string partial, int replaceStart, int replaceEnd)
	{
		var allNames = CommandRegistry.Instance.GetCommandNames();
		var filtered = new List<string>();

		foreach (var name in allNames)
		{
			if (string.IsNullOrEmpty(partial) ||
				name.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
			{
				filtered.Add(name);
			}
		}

		filtered.Sort(StringComparer.OrdinalIgnoreCase);

		return new()
		{
			Suggestions = filtered.Count > 0 ? filtered : null,
			ReplaceStart = replaceStart,
			ReplaceEnd = replaceEnd
		};
	}
}
}
