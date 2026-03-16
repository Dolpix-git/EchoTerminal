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
			var matches = new List<string>();
			foreach (var name in _parser.Registry.GetCommandNames())
			{
				if (name.StartsWith(result.CommandName, StringComparison.OrdinalIgnoreCase))
				{
					matches.Add(name);
				}
			}

			return matches.Count == 0
				? SuggestionContext.Empty
				: new() { Suggestions = matches, ReplaceStart = result.LeadingSpaces, ReplaceEnd = input.Length };
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
					if (seen.Add(name) && name.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
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

	private static SuggestionContext SuggestParam(string input, CommandParseResult result)
	{
		if (result.Overloads.Count == 0)
		{
			return SuggestionContext.Empty;
		}

		var best = PickBestOverload(result.Overloads);

		foreach (var param in best.Params)
		{
			if (param.Expected.IsTarget)
			{
				if (param.IsValid || param.Token == null)
				{
					continue;
				}

				return SuggestionContext.Empty;
			}

			if (param.IsValid)
			{
				continue;
			}

			var lastSpace = input.LastIndexOf(' ');
			var replaceStart = lastSpace + 1;
			var partial = replaceStart < input.Length ? input[replaceStart..] : "";

			var matches = GetTypeSuggestions(param.Expected.Type, partial);
			if (matches == null || matches.Count == 0)
			{
				return SuggestionContext.Empty;
			}

			return new() { Suggestions = matches, ReplaceStart = replaceStart, ReplaceEnd = input.Length };
		}

		return SuggestionContext.Empty;
	}

	private static List<string> GetTypeSuggestions(Type type, string partial)
	{
		// Exact type match first.
		if (!Suggestors.TryGetValue(type, out var suggestor))
		{
			// For enums, fall back to the EnumSuggestor registered under typeof(Enum).
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