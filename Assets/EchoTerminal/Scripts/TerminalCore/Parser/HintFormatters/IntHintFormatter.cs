using System;

namespace EchoTerminal.Scripts.Test
{
public class IntHintFormatter : IHintFormatter
{
	public Type TargetType => typeof(int);
	public string Format => "0";
}
}
