using System;
using UnityEngine;

namespace EchoTerminal
{
public interface IValueParser
{
	Type TargetType { get; }
	char? OpenDelimiter { get; }
	char? CloseDelimiter { get; }
	Color HighlightColor { get; }
	string Hint { get; }
	string[] Suggestions { get; }
	bool TryParse(string token, out object result);
}
}
