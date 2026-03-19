using System;
using UnityEngine;

namespace EchoTerminal
{
public readonly struct TerminalEntry
{
	public readonly string Text;
	public readonly Color Color;
	public readonly DateTime Timestamp;
	public readonly LogKind Kind;

	public TerminalEntry(string text, Color color, LogKind kind = LogKind.Log)
	{
		Text = text;
		Color = color;
		Timestamp = DateTime.Now;
		Kind = kind;
	}
}
}
