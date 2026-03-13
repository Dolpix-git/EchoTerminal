using System;
using System.Globalization;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class Vector3Parser : IParser
{
	public Type TargetType => typeof(Vector3);

	public bool TryParse(string input, out object result, out int charsConsumed)
	{
		result = null;
		charsConsumed = 0;

		var end = input.IndexOf(' ');
		var token = end == -1 ? input : input.Substring(0, end);

		var parts = token.Split(',');
		if (parts.Length != 3)
		{
			return false;
		}

		if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
			!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
			!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
		{
			return false;
		}

		result = new Vector3(x, y, z);
		charsConsumed = token.Length;
		return true;
	}
}
}