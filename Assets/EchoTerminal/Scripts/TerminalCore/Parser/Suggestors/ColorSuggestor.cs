using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class ColorSuggestor : ISuggestor
{
	private static readonly string[] Values =
	{
		"red", "green", "blue", "white", "black",
		"yellow", "cyan", "magenta", "grey", "clear"
	};

	public Type TargetType => typeof(Color);

	public IReadOnlyList<string> GetSuggestions(Type type, string partial)
	{
		return Values.Where(v => v.StartsWith(partial, StringComparison.OrdinalIgnoreCase)).ToList();
	}
}
}