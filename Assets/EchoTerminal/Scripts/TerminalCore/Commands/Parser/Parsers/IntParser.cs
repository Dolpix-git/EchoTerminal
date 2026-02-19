using System;
using System.Globalization;
using UnityEngine;

namespace EchoTerminal
{
public class IntParser : IValueParser
{
	public Type TargetType => typeof(int);
	public char? OpenDelimiter => null;
	public char? CloseDelimiter => null;
	public Color HighlightColor => new(0.7f, 0.55f, 1f);
	public string Hint => "integer";
	public string[] Suggestions => null;

	public bool TryParse(string token, out object result)
	{
		result = null;

		if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
		{
			return false;
		}

		result = value;
		return true;
	}
}
}
