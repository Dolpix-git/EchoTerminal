using System.Collections.Generic;
using EchoTerminal;
using UnityEngine;

namespace DefaultNamespace
{
public class ExampleMono : MonoBehaviour
{
	[TerminalCommand]
	private void Teleport(float x, float y, float z)
	{
		transform.position = new(x, y, z);
		Debug.Log("New position: " + transform.position);
	}
	
	[TerminalCommand]
	private void Teleport(Vector3 position)
	{
		transform.position = new(position.x, position.y, position.z);
		Debug.Log("New position: " + transform.position);
	}

	[TerminalCommand("hp")]
	private string SetHealth(int value)
	{
		return $"Health set to {value}";
	}

	[TerminalCommand]
	private string Say(string message)
	{
		return $"[{gameObject.name}]: {message}";
	}

	[TerminalCommand]
	private string Toggle(bool enabled)
	{
		gameObject.SetActive(enabled);
		return $"{gameObject.name} active: {enabled}";
	}

	[TerminalCommand]
	private string Move(Vector3 offset)
	{
		transform.position += offset;
		return $"Moved to {transform.position}";
	}

	[TerminalCommand]
	private string Look(Vector2 direction)
	{
		return $"Looking at ({direction.x}, {direction.y})";
	}

	[TerminalCommand]
	private string Paint(Color color)
	{
		var renderer = GetComponentInChildren<MeshRenderer>();

		if (renderer != null)
		{
			renderer.sharedMaterial.color = color;
		}

		return $"Color set to {color}";
	}

	[TerminalCommand]
	private string Speed(double value)
	{
		return $"Speed set to {value:F4}";
	}

	[TerminalCommand]
	private string Damage(List<int> amounts)
	{
		var total = 0;

		foreach (var amount in amounts)
		{
			total += amount;
		}

		return $"Dealt {amounts.Count} hits for {total} total damage";
	}

	[TerminalCommand]
	private string Waypoints(List<Vector3> points)
	{
		var sb = new System.Text.StringBuilder();
		sb.Append($"Set {points.Count} waypoints:");

		for (var i = 0; i < points.Count; i++)
		{
			sb.Append($" [{i}]{points[i]}");
		}

		return sb.ToString();
	}
}
}
