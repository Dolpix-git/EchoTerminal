using System.Collections.Generic;

namespace EchoTerminal
{
public struct SuggestionContext
{
	public static readonly SuggestionContext Empty = new();
	public List<string> Suggestions;
	public int ReplaceStart;
	public int ReplaceEnd;
}
}
