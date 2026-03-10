using System;

namespace EchoTerminal
{
[AttributeUsage(AttributeTargets.Method)]
public sealed class TerminalCommandAttribute : Attribute
{
	public string Name { get; }

	public TerminalCommandAttribute(string name = null, string description = null)
	{
		Name = name;
		Description = description;
	}

	public string Description { get; }
}
}