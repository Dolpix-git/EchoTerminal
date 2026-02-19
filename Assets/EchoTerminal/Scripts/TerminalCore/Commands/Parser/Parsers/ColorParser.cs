using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EchoTerminal
{
public class ColorParser : IValueParser
{
	private static readonly Dictionary<string, Color> NamedColors = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "red", Color.red },
		{ "green", Color.green },
		{ "blue", Color.blue },
		{ "white", Color.white },
		{ "black", Color.black },
		{ "yellow", Color.yellow },
		{ "cyan", Color.cyan },
		{ "magenta", Color.magenta },
		{ "gray", Color.gray },
		{ "grey", Color.grey },
		{ "clear", Color.clear },
	};

	public Type TargetType => typeof(Color);
	public char? OpenDelimiter => '(';
	public char? CloseDelimiter => ')';
	public Color HighlightColor => new(1f, 0.5f, 0.7f);
	public string Hint => "(r,g,b) | #hex | name";

	public string[] Suggestions =>
		new[] { "red", "green", "blue", "white", "black", "yellow", "cyan", "magenta", "grey" };

	public bool TryParse(string token, out object result)
	{
		result = null;

		if (token.Length == 0)
		{
			return false;
		}

		switch (token[0])
		{
			case '#':
				return TryParseHex(token, out result);
			case '(' when token[^1] == ')':
				return TryParseComponents(token, out result);
		}

		if (!NamedColors.TryGetValue(token, out var named))
		{
			return false;
		}

		result = named;
		return true;
	}

	private static bool TryParseHex(string token, out object result)
	{
		result = null;
		var hex = token.Substring(1);

		if (hex.Length != 6 && hex.Length != 8)
		{
			return false;
		}

		if (!byte.TryParse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r))
		{
			return false;
		}

		if (!byte.TryParse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g))
		{
			return false;
		}

		if (!byte.TryParse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
		{
			return false;
		}

		var a = (byte)255;

		if (hex.Length == 8 &&
			!byte.TryParse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a))
		{
			return false;
		}

		result = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
		return true;
	}

	private static bool TryParseComponents(string token, out object result)
	{
		result = null;
		var inner = token.Substring(1, token.Length - 2);
		var parts = inner.Split(',');

		if (parts.Length != 3 && parts.Length != 4)
		{
			return false;
		}

		if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var r))
		{
			return false;
		}

		if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var g))
		{
			return false;
		}

		if (!float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
		{
			return false;
		}

		var a = 1f;

		if (parts.Length == 4 &&
			!float.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out a))
		{
			return false;
		}

		result = new Color(r, g, b, a);
		return true;
	}
}
}