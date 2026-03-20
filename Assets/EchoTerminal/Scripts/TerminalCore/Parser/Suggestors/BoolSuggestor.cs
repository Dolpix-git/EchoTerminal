using System;
using System.Collections.Generic;

namespace EchoTerminal.Scripts.Test
{
public class BoolSuggestor : ISuggestor
{
	private static readonly string[] Values = { "true", "false" };

	public Type TargetType => typeof(bool);

	public IReadOnlyList<string> GetSuggestions(Type type, string partial)
	{
		return FuzzyMatcher.Filter(Values, partial);
	}
}
}