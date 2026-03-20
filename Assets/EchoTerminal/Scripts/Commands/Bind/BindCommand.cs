using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoTerminal
{
public static class BindCommand
{
	[TerminalCommand("BindsAdd", "Bind a key to a command")]
	private static void BindsAdd(Key key, CommandName command)
	{
		BindStore.Set(key, command.Value);
		Debug.Log($"Bound {key} to: {command.Value}");
	}

	[TerminalCommand("BindsRemove", "Remove a key binding")]
	private static void BindsRemove(Key key)
	{
		Debug.Log(BindStore.Remove(key) ? $"Unbound {key}" : $"No bind found for '{key}'");
	}

	[TerminalCommand("BindsClear", "Remove all key bindings")]
	private static void BindsClear()
	{
		BindStore.Clear();
		Debug.Log("All binds removed.");
	}

	[TerminalCommand("BindsLog", "List all current key bindings")]
	private static void BindsLog()
	{
		var all = BindStore.GetAll();

		if (all.Count == 0)
		{
			Debug.Log("No binds set.");
			return;
		}

		var sb = new StringBuilder();
		sb.AppendLine("Current binds:");

		foreach (var (key, command) in all)
		{
			sb.AppendLine($"  {key} → {command}");
		}

		Debug.Log(sb.ToString().TrimEnd());
	}
}
}