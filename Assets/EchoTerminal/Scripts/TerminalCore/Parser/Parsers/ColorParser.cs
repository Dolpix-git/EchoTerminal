using System;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class ColorParser : IParser
{
	public Type TargetType => typeof(Color);

	public bool TryParse(string input, out object result, out int charsConsumed)
	{
		result = null;
		charsConsumed = 0;

		var end = input.IndexOf(' ');
		var token = end == -1 ? input : input[..end];

		if (!TryParseNamed(token, out var color) && !TryParseHex(token, out color))
		{
			return false;
		}

		result = color;
		charsConsumed = token.Length;
		return true;
	}

	private static bool TryParseNamed(string token, out Color color)
	{
		switch (token.ToLowerInvariant())
		{
			case "red":
				color = Color.red;
				return true;
			case "green":
				color = Color.green;
				return true;
			case "blue":
				color = Color.blue;
				return true;
			case "white":
				color = Color.white;
				return true;
			case "black":
				color = Color.black;
				return true;
			case "yellow":
				color = Color.yellow;
				return true;
			case "cyan":
				color = Color.cyan;
				return true;
			case "magenta":
				color = Color.magenta;
				return true;
			case "grey":
			case "gray":
				color = Color.grey;
				return true;
			case "clear":
				color = Color.clear;
				return true;
			default:
				color = default;
				return false;
		}
	}

	private static bool TryParseHex(string token, out Color color)
	{
		color = default;
		return token.StartsWith("#") && ColorUtility.TryParseHtmlString(token, out color);
	}
}
}