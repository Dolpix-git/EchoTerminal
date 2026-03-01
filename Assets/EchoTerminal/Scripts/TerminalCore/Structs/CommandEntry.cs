using System;
using System.Reflection;

namespace EchoTerminal
{
public readonly struct CommandEntry
{
	public readonly MethodInfo Method;
	public readonly Type MonoType;

	public bool IsStatic => MonoType == null;

	public CommandEntry(MethodInfo method, Type monoType)
	{
		Method = method;
		MonoType = monoType;
	}
}
}