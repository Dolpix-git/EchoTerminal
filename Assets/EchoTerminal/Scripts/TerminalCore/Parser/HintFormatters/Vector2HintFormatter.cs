using System;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class Vector2HintFormatter : IHintFormatter
{
	public Type TargetType => typeof(Vector2);
	public string Format => "0,0";
}
}
