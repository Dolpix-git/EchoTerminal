using System;
using System.Globalization;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class Vector2Parser : IParser
{
	public Type TargetType => typeof(Vector2);

	public bool TryParse(string input, out object result, out int charsConsumed)
	{
		result = null;
		charsConsumed = 0;

		var end = input.IndexOf(' ');
		var token = end == -1 ? input : input.Substring(0, end);

		var parts = token.Split(',');
		if (parts.Length != 2)
		{
			return false;
		}

		if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
			!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
		{
			return false;
		}

		result = new Vector2(x, y);
		charsConsumed = token.Length;
		return true;
	}
}
}