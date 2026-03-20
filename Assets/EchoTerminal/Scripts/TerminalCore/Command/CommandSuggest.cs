using System;
using System.Collections.Generic;
using System.Reflection;
using EchoTerminal.Scripts.Test;

namespace EchoTerminal
{
public class CommandSuggest
{
	private static Dictionary<Type, ISuggestor> _suggestors;

	private readonly CommandParser _parser;

	public CommandSuggest(CommandParser parser)
	{
		_parser = parser;
	}

	private static IReadOnlyDictionary<Type, ISuggestor> Suggestors => GetSuggestors();

	private static Dictionary<Type, ISuggestor> GetSuggestors()
	{
		if (_suggestors != null)
		{
			return _suggestors;
		}

		_suggestors = new();

		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.IsDynamic)
			{
				continue;
			}

			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = e.Types;
			}

			if (types == null)
			{
				continue;
			}

			foreach (var type in types)
			{
				if (type == null || type.IsAbstract || type.IsInterface)
				{
					continue;
				}

				if (!typeof(ISuggestor).IsAssignableFrom(type))
				{
					continue;
				}

				if (type.GetConstructor(Type.EmptyTypes) == null)
				{
					continue;
				}

				var suggestor = (ISuggestor)Activator.CreateInstance(type);
				_suggestors[suggestor.TargetType] = suggestor;
			}
		}

		return _suggestors;
	}

	public SuggestionContext GetSuggestions(string input)
	{
		var result = _parser.Parse(input);

		if (string.IsNullOrEmpty(result.CommandName))
		{
			return SuggestionContext.Empty;
		}

		if (result.Args == null)
		{
			var matches = FuzzyMatcher.Filter(_parser.Registry.GetCommandNames(), result.CommandName);
			if (matches.Count == 0)
			{
				return SuggestionContext.Empty;
			}

			return new() { Suggestions = matches, ReplaceStart = result.LeadingSpaces, ReplaceEnd = input.Length };
		}

		if (result.Args.StartsWith("@") && !result.Args[1..].Contains(' '))
		{
			var partial = result.Args[1..];
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var goMatches = new List<string>();

			foreach (var overload in result.Overloads)
			{
				if (overload.Params.Count == 0 || !overload.Params[0].Expected.IsTarget)
				{
					continue;
				}

				foreach (var component in _parser.Registry.GetInstances(overload.Entry.MonoType))
				{
					var name = component.gameObject.name;
					if (seen.Add(name) && FuzzyMatcher.Score(name, partial) >= 0)
					{
						goMatches.Add(name);
					}
				}
			}

			if (goMatches.Count == 0)
			{
				return SuggestionContext.Empty;
			}

			var atPos = input.IndexOf('@', result.LeadingSpaces + result.CommandName.Length);
			return new() { Suggestions = goMatches, ReplaceStart = atPos + 1, ReplaceEnd = input.Length };
		}

		return SuggestParam(input, result);
	}

	private SuggestionContext SuggestParam(string input, CommandParseResult result, int depth = 0)
	{
		if (result.Overloads.Count == 0)
		{
			return SuggestionContext.Empty;
		}

		var best = PickBestOverload(result.Overloads);
		var hasPrevValid = false;
		var prevValidParam = default(ParamResult);

		foreach (var param in best.Params)
		{
			if (param.Expected.IsTarget)
			{
				if (param.IsValid || param.Token == null)
				{
					if (param.IsValid)
					{
						hasPrevValid = true;
						prevValidParam = param;
					}

					continue;
				}

				return SuggestionContext.Empty;
			}

			if (param.IsValid)
			{
				hasPrevValid = true;
				prevValidParam = param;
				continue;
			}

			if (param.Token == null && hasPrevValid && !input.EndsWith(" "))
			{
				var lastSp = input.LastIndexOf(' ');
				var rStart = lastSp + 1;
				var prevPartial = rStart < input.Length ? input[rStart..] : "";
				var prevMatches = GetTypeSuggestions(prevValidParam.Expected.Type, prevPartial);
				if (prevMatches != null && prevMatches.Count > 0)
				{
					return new() { Suggestions = prevMatches, ReplaceStart = rStart, ReplaceEnd = input.Length };
				}
			}

			if (param.Expected.Type == typeof(CommandName))
			{
				return depth == 0 ? SuggestCommandName(input) : SuggestionContext.Empty;
			}

			var lastSpace = input.LastIndexOf(' ');
			var replaceStart = lastSpace + 1;
			var partial = replaceStart < input.Length ? input[replaceStart..] : "";

			if (param.Expected.Type.IsGenericType &&
				param.Expected.Type.GetGenericTypeDefinition() == typeof(List<>))
			{
				return SuggestListElement(param.Expected.Type, partial, replaceStart, input.Length);
			}

			var matches = GetTypeSuggestions(param.Expected.Type, partial);
			if (matches == null || matches.Count == 0)
			{
				return SuggestionContext.Empty;
			}

			return new() { Suggestions = matches, ReplaceStart = replaceStart, ReplaceEnd = input.Length };
		}

		return SuggestionContext.Empty;
	}

	private SuggestionContext SuggestCommandName(string input)
	{
		var openBracket = -1;
		for (var i = input.Length - 1; i >= 0; i--)
		{
			if (input[i] == '<')
			{
				break;
			}

			if (input[i] == '>')
			{
				openBracket = i;
				break;
			}
		}

		int tokenStart;
		string innerInput;

		if (openBracket >= 0)
		{
			tokenStart = openBracket;
			innerInput = input[(openBracket + 1)..];
		}
		else
		{
			var lastSpace = input.LastIndexOf(' ');
			tokenStart = lastSpace >= 0 ? lastSpace + 1 : 0;
			innerInput = "";
		}

		if (!CommandProcessor.TryParseInput(innerInput, out var innerCmd, out var innerArgs, out _) ||
			innerCmd.Length == 0)
		{
			return AllCommandSuggestions(tokenStart, input.Length);
		}

		if (innerArgs == null)
		{
			return FilteredCommandSuggestions(innerCmd, tokenStart, input.Length);
		}

		var innerResult = _parser.Parse(innerInput);
		var innerCtx = SuggestParam(innerInput, innerResult, 1);

		if (innerCtx.Suggestions == null || innerCtx.Suggestions.Count == 0)
		{
			return SuggestionContext.Empty;
		}

		var innerPrefix = innerInput[..innerCtx.ReplaceStart];
		var innerSuffix = innerCtx.ReplaceEnd < innerInput.Length
			? innerInput[innerCtx.ReplaceEnd..]
			: "";

		var wrapped = new List<string>(innerCtx.Suggestions.Count);
		foreach (var s in innerCtx.Suggestions)
		{
			wrapped.Add($">{innerPrefix}{s}{innerSuffix}<");
		}

		return new() { Suggestions = wrapped, ReplaceStart = tokenStart, ReplaceEnd = input.Length };
	}

	private SuggestionContext AllCommandSuggestions(int replaceStart, int replaceEnd)
	{
		var names = new List<string>(_parser.Registry.GetCommandNames());
		names.Sort(StringComparer.OrdinalIgnoreCase);

		var wrapped = new List<string>(names.Count);
		foreach (var n in names)
		{
			wrapped.Add($">{n}<");
		}

		return wrapped.Count == 0
			? SuggestionContext.Empty
			: new() { Suggestions = wrapped, ReplaceStart = replaceStart, ReplaceEnd = replaceEnd };
	}

	private SuggestionContext FilteredCommandSuggestions(string partial, int replaceStart, int replaceEnd)
	{
		var names = FuzzyMatcher.Filter(_parser.Registry.GetCommandNames(), partial);
		if (names.Count == 0)
		{
			return SuggestionContext.Empty;
		}

		var wrapped = new List<string>(names.Count);
		foreach (var n in names)
		{
			wrapped.Add($">{n}<");
		}

		return new() { Suggestions = wrapped, ReplaceStart = replaceStart, ReplaceEnd = replaceEnd };
	}

	private static SuggestionContext SuggestListElement(
		Type listType,
		string partial,
		int replaceStart,
		int replaceEnd)
	{
		var elemType = listType.GetGenericArguments()[0];
		var lastComma = partial.LastIndexOf(',');
		var elemPartial = lastComma == -1 ? partial : partial[(lastComma + 1)..].TrimStart();
		var prefix = lastComma == -1 ? "" : partial[..(lastComma + 1)];

		var elemMatches = GetTypeSuggestions(elemType, elemPartial);
		if (elemMatches == null || elemMatches.Count == 0)
		{
			return SuggestionContext.Empty;
		}

		var suggestions = new List<string>(elemMatches.Count);
		foreach (var s in elemMatches)
		{
			suggestions.Add(prefix + s);
		}

		return new() { Suggestions = suggestions, ReplaceStart = replaceStart, ReplaceEnd = replaceEnd };
	}

	private static List<string> GetTypeSuggestions(Type type, string partial)
	{
		if (!Suggestors.TryGetValue(type, out var suggestor))
		{
			if (!type.IsEnum || !Suggestors.TryGetValue(typeof(Enum), out suggestor))
			{
				return null;
			}
		}

		var results = suggestor.GetSuggestions(type, partial);
		return results == null || results.Count == 0 ? null : new List<string>(results);
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
}
}