using System;
using System.Globalization;
using UnityEngine;

namespace EchoTerminal
{
public class Vector3Parser : IValueParser
{
	public Type TargetType => typeof(Vector3);
	public char? OpenDelimiter => '(';
	public char? CloseDelimiter => ')';
	public Color HighlightColor => new(0.5f, 0.7f, 1f);
	public string Hint => "(x,y,z)";
	public string[] Suggestions => null;

	public bool TryParse(string token, out object result)
	{
		result = null;

		if (token.Length < 2 || token[0] != '(' || token[^1] != ')')
		{
			return false;
		}

		var inner = token.Substring(1, token.Length - 2);
		var parts = inner.Split(',');

		if (parts.Length != 3)
		{
			return false;
		}

		if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
		{
			return false;
		}

		if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
		{
			return false;
		}

		if (!float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
		{
			return false;
		}

		result = new Vector3(x, y, z);
		return true;
	}
}
}