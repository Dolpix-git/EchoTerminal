using System;
using UnityEngine;

namespace EchoTerminal
{
public class EnumParser : IValueParser
{
	private readonly Type _enumType;

	public EnumParser(Type enumType) => _enumType = enumType;

	public Type TargetType => _enumType;
	public char? OpenDelimiter => null;
	public char? CloseDelimiter => null;
	public Color HighlightColor => new(1f, 0.6f, 0.2f);
	public string Hint => _enumType.ToString();
	public string[] Suggestions => Enum.GetNames(_enumType);

	public bool TryParse(string token, out object result)
	{
		try
		{
			result = Enum.Parse(_enumType, token, true);
			return true;
		}
		catch
		{
			result = null;
			return false;
		}
	}
}
}
