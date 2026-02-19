using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EchoTerminal;
using UnityEngine;

public static class HelpCommand
{
	private const string _colCommand = "#79C0FF";
	private const string _colParams  = "#6E8FA8";
	private const string _colArrow   = "#4A6274";
	private const string _colDesc    = "#9EAAB5";
	private const string _colHeader  = "#C9D1D9";

	[TerminalCommand(description: "Show all registered commands")]
	private static void Help()
	{
		var registry = CommandRegistry.Instance;
		var names = registry.GetCommandNames();

		if (names.Count == 0)
		{
			Debug.Log("No commands registered.");
			return;
		}

		var sorted = new List<string>(names);
		sorted.Sort();

		var output = new StringBuilder();
		output.AppendLine($"  <color={_colHeader}>Available Commands</color>");
		output.AppendLine(" ");

		foreach (var name in sorted)
		{
			var matches = registry.FindCommands(name, null);
			var seen = new HashSet<string>();

			foreach (var (_, method) in matches)
			{
				var line = BuildLine(name, method);

				if (seen.Add(line))
				{
					output.AppendLine(line);
				}
			}
		}

		Debug.Log(output.ToString().TrimEnd());
	}

	private static string BuildLine(string name, MethodInfo method)
	{
		var sb = new StringBuilder();
		sb.Append($"  <color={_colCommand}>{name}</color>");

		foreach (var param in method.GetParameters())
		{
			sb.Append($" <color={_colParams}><{param.ParameterType.Name} {param.Name}></color>");
		}

		var desc = method.GetCustomAttribute<TerminalCommandAttribute>()?.Description;
		if (!string.IsNullOrEmpty(desc))
		{
			sb.Append($"  <color={_colArrow}>→</color>  <color={_colDesc}>{desc}</color>");
		}

		return sb.ToString();
	}
}
