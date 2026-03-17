using System;

namespace EchoTerminal.Scripts.Test
{
public class FloatHintFormatter : IHintFormatter
{
	public Type TargetType => typeof(float);
	public string Format => "0.0";
}
}
