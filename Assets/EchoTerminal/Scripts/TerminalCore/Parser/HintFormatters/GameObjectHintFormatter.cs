using System;
using UnityEngine;

namespace EchoTerminal.Scripts.Test
{
public class GameObjectHintFormatter : IHintFormatter
{
	public Type TargetType => typeof(GameObject);
	public string Format => "name";
}
}
