using System;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class Vector3HintFormatter : IHintFormatter
{
	public Type TargetType => typeof(Vector3);
	public string Format => "0,0,0";
}
}
