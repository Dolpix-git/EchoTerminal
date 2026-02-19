using System.Collections.Generic;
using System.Reflection;

namespace EchoTerminal
{
public class InputAnalysis
{
	public List<(string token, int start, int end)> Spans;
	public bool TrailingSpace;
	public int EditingIndex;
	public string Partial;
	public int ReplaceStart;
	public int ReplaceEnd;

	public string CommandName;
	public bool CommandValid;
	public bool HasTarget;
	public int ArgStart;
	public ParameterInfo[] Parameters;

	public int SubCommandOffset = -1;
}
}
