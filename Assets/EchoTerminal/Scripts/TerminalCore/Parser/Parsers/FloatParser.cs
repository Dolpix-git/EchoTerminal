using System;
using System.Globalization;

namespace EchoTerminal.Scripts.Test
{
public class FloatParser : IParser
{
	public Type TargetType => typeof(float);

	public bool TryParse(string input, out object result, out int charsConsumed)
	{
		var end = input.IndexOf(' ');
		var token = end == -1 ? input : input.Substring(0, end);

		if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
		{
			result = value;
			charsConsumed = token.Length;
			return true;
		}

		result = null;
		charsConsumed = 0;
		return false;
	}
}
}