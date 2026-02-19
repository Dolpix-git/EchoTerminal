using System.Collections.Generic;

namespace EchoTerminal
{
public class AutocompleteContext
{
	public static readonly AutocompleteContext Empty = new();
	public List<string> Suggestions;
	public int ReplaceStart;
	public int ReplaceEnd;
}
}
