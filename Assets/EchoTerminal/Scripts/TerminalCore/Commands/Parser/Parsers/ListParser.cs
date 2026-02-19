using System;
using UnityEngine;

namespace EchoTerminal
{
public class ListParser : IValueParser
{
	public Type TargetType => typeof(System.Collections.IList);
	public char? OpenDelimiter => '[';
	public char? CloseDelimiter => ']';
	public Color HighlightColor => new(0.4f, 0.9f, 0.8f);
	public string Hint => "[item,item,...]";
	public string[] Suggestions => null;

	public bool TryParse(string token, out object result)
	{
		result = null;
		return false;
	}
}
}
