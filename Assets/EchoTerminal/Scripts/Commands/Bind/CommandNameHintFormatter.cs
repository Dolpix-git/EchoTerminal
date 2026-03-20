using System;
using EchoTerminal.Scripts.Test;

namespace EchoTerminal
{
public class CommandNameHintFormatter : IHintFormatter
{
	public Type TargetType => typeof(CommandName);
	public string Format => ">command<";
}
}