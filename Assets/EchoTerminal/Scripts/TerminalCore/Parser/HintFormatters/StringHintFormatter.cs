using System;

namespace EchoTerminal.Scripts.Test
{
public class StringHintFormatter : IHintFormatter
{
	public Type TargetType => typeof(string);
	public string Format => "\"text\"";
}
}
