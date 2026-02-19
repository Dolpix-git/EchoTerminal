using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoTerminal
{
public static class BindCommand
{
	[TerminalCommand("bind")]
	private static void Bind(Key key, string command)
	{
		BindStore.Set(key, command);
		Debug.Log($"Bound {key} to: {command}");
	}

	[TerminalCommand("unbind")]
	private static void Unbind(Key key)
	{
		if (BindStore.Remove(key))
		{
			Debug.Log($"Unbound {key}");
		}
		else
		{
			Debug.Log($"No bind found for '{key}'");
		}
	}

	[TerminalCommand("binds")]
	private static void Binds()
	{
		var all = BindStore.GetAll();

		if (all.Count == 0)
		{
			Debug.Log("No binds set.");
			return;
		}

		var sb = new System.Text.StringBuilder();
		sb.AppendLine("Current binds:");

		foreach (var (key, command) in all)
		{
			sb.AppendLine($"  {key} → {command}");
		}

		Debug.Log(sb.ToString().TrimEnd());
	}
}
}
