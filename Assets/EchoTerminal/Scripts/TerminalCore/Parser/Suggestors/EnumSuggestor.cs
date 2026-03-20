using System;
using System.Collections.Generic;

namespace EchoTerminal.Scripts.Test
{
public class EnumSuggestor : ISuggestor
{
	public Type TargetType => typeof(Enum);

	public IReadOnlyList<string> GetSuggestions(Type type, string partial)
	{
		return FuzzyMatcher.Filter(Enum.GetNames(type), partial);
	}
}
}