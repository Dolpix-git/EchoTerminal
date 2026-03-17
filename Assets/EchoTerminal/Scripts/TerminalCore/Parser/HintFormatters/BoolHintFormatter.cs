using System;

namespace EchoTerminal.Scripts.Test
{
public class BoolHintFormatter : IHintFormatter
{
	public Type TargetType => typeof(bool);
	public string Format => "true";
}
}
