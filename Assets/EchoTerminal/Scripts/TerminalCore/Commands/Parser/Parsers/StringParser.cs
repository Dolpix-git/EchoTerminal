using System;
using UnityEngine;

namespace EchoTerminal
{
public class StringParser : IValueParser
{
	public Type TargetType => typeof(string);
	public char? OpenDelimiter => '"';
	public char? CloseDelimiter => '"';
	public Color HighlightColor => new(0.6f, 0.9f, 0.3f);
	public string Hint => "\"text\"";
	public string[] Suggestions => null;

	public bool TryParse(string token, out object result)
	{
		if (token.Length >= 2 && token[0] == '"' && token[^1] == '"')
		{
			result = token.Substring(1, token.Length - 2);
		}
		else
		{
			result = token;
		}

		return true;
	}
}
}