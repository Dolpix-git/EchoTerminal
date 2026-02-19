using System;
using UnityEngine;

namespace EchoTerminal
{
public readonly struct TerminalEntry
{
	public readonly string Text;
	public readonly Color Color;
	public readonly DateTime Timestamp;

	public TerminalEntry(string text, Color color)
	{
		Text = text;
		Color = color;
		Timestamp = DateTime.Now;
	}
}
}
