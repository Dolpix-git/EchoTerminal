using System;

namespace EchoTerminal.Scripts.Test
{
public interface IHintFormatter
{
	Type TargetType { get; }
	string Format { get; }
}
}
