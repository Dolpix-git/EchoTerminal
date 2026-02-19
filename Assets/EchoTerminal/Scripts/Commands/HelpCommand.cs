using System.Text;
using EchoTerminal;
using UnityEngine;

public static class HelpCommand
{
	[TerminalCommand]
	private static void Help()
	{
		var registry = CommandRegistry.Instance;
		var names = registry.GetCommandNames();

		if (names.Count == 0)
		{
			Debug.Log("No commands available.");
			return;
		}

		var sb = new StringBuilder();
		sb.AppendLine("Available commands:");

		foreach (var name in names)
		{
			var matches = registry.FindCommands(name, null);
			var seen = new System.Collections.Generic.HashSet<string>();

			foreach (var (_, method) in matches)
			{
				var sig = new StringBuilder();
				sig.Append("  ");
				sig.Append(name);

				var parameters = method.GetParameters();
				foreach (var param in parameters)
				{
					sig.Append($" <{param.ParameterType.Name} {param.Name}>");
				}

				var line = sig.ToString();
				if (seen.Add(line))
				{
					sb.AppendLine(line);
				}
			}
		}

		Debug.Log(sb.ToString().TrimEnd());
	}
}
