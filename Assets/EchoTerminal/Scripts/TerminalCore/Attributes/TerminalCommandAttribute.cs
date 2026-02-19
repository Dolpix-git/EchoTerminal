using System;

namespace EchoTerminal
{
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class TerminalCommandAttribute : Attribute
{
	public string Name { get; }

	public TerminalCommandAttribute(string name = null)
	{
		Name = name;
	}
}
}
