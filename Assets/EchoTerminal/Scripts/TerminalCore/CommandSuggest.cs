using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EchoTerminal
{
public class CommandSuggest
{
	private readonly CommandRegistry _registry;

	public CommandSuggest(CommandRegistry registry)
	{
		_registry = registry;
	}

	public AutocompleteContext GetSuggestions(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return AutocompleteContext.Empty;
		}

		var trimmed = input.TrimStart();
		var leadingSpaces = input.Length - trimmed.Length;
		var space = trimmed.IndexOf(' ');

		if (space == -1)
		{
			var matches = new List<string>();
			foreach (var name in _registry.GetCommandNames())
			{
				if (name.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase))
				{
					matches.Add(name);
				}
			}

			if (matches.Count == 0)
			{
				return AutocompleteContext.Empty;
			}

			return new()
			{
				Suggestions = matches,
				ReplaceStart = leadingSpaces,
				ReplaceEnd = input.Length
			};
		}

		var afterCommand = trimmed[(space + 1)..];
		if (afterCommand.StartsWith("@") && !afterCommand[1..].Contains(' '))
		{
			var partial = afterCommand[1..];
			var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
			var goMatches = new List<string>();
			foreach (var go in allGOs)
			{
				if (go.name.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
				{
					goMatches.Add(go.name);
				}
			}

			if (goMatches.Count == 0)
			{
				return AutocompleteContext.Empty;
			}

			var atPos = input.IndexOf('@', leadingSpaces + space);
			return new()
			{
				Suggestions = goMatches,
				ReplaceStart = atPos + 1,
				ReplaceEnd = input.Length
			};
		}

		return AutocompleteContext.Empty;
	}

	public List<string> GetHints(string input)
	{
		var trimmed = input.TrimStart();
		var space = trimmed.IndexOf(' ');
		if (space == -1)
		{
			return null;
		}

		var commandName = trimmed[..space];
		if (!_registry.TryGet(commandName, out var entries))
		{
			return null;
		}

		var hints = new List<string>();
		foreach (var entry in entries)
		{
			var parameters = entry.Method.GetParameters();
			var parts = new List<string>();

			if (!entry.IsStatic)
			{
				parts.Add("<@gameObject>");
			}

			foreach (var p in parameters)
			{
				if (p.ParameterType == typeof(Terminal))
				{
					continue;
				}

				parts.Add($"<{p.Name}:{p.ParameterType.Name}>");
			}

			if (parts.Count > 0)
			{
				hints.Add(string.Join(" ", parts));
			}
		}

		return hints.Count > 0 ? hints : null;
	}
}
}