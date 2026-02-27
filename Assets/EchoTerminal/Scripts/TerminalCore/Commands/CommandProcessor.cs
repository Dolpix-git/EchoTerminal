using System;
using System.Collections.Generic;
using System.Reflection;
using EchoTerminal.Scripts.Test;

namespace EchoTerminal
{
public class CommandProcessor
{
	private static readonly Dictionary<Type, IParser> Parsers = new()
	{
		{ typeof(int), new IntParser() },
		{ typeof(bool), new BoolParser() },
		{ typeof(string), new StringParser() }
	};

	private readonly Terminal _terminal;

	public CommandProcessor(Terminal terminal)
	{
		_terminal = terminal;
	}

	public void Execute(string input)
	{
		var remaining = input.TrimStart();
		if (remaining.Length == 0)
		{
			return;
		}

		var space = remaining.IndexOf(' ');
		var commandName = space == -1 ? remaining : remaining.Substring(0, space);
		remaining = space == -1 ? string.Empty : remaining.Substring(space).TrimStart();

		if (!CommandRegistry.Instance.TryGet(commandName, out var method))
		{
			_terminal.Log($"Unknown command: '{commandName}'");
			return;
		}

		if (!TryInvoke(method, remaining))
		{
			_terminal.Log($"Invalid arguments for '{commandName}'");
		}
	}

	private static bool TryInvoke(MethodInfo method, string remaining)
	{
		var parameters = method.GetParameters();
		var args = new object[parameters.Length];

		for (var i = 0; i < parameters.Length; i++)
		{
			if (!Parsers.TryGetValue(parameters[i].ParameterType, out var parser))
			{
				return false;
			}

			if (!parser.TryParse(remaining, out args[i], out var consumed))
			{
				return false;
			}

			remaining = remaining.Substring(consumed).TrimStart();
		}

		method.Invoke(null, args);
		return true;
	}
}
}