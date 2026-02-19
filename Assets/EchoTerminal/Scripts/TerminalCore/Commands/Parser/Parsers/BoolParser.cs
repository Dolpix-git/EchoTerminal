using System;
using UnityEngine;

namespace EchoTerminal
{
public class BoolParser : IValueParser
{
	public Type TargetType => typeof(bool);
	public char? OpenDelimiter => null;
	public char? CloseDelimiter => null;
	public Color HighlightColor => new(1f, 0.6f, 0.2f);
	public string Hint => "true | false";
	public string[] Suggestions => new[] { "true", "false" };

	public bool TryParse(string token, out object result)
	{
		result = null;

		if (!bool.TryParse(token, out var value))
		{
			return false;
		}

		result = value;
		return true;
	}
}
}
