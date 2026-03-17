using System;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class ColorHintFormatter : IHintFormatter
{
	public Type TargetType => typeof(Color);
	public string Format => "red|#RRGGBB";
}
}
