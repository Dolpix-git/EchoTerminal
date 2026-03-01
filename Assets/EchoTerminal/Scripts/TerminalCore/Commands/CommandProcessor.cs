using System;
using System.Collections.Generic;
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
	private readonly CommandRegistry _registry;

	public CommandProcessor(Terminal terminal, CommandRegistry registry)
	{
		_terminal = terminal;
		_registry = registry;
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

		if (!_registry.TryGet(commandName, out var entry))
		{
			_terminal.Log($"Unknown command: '{commandName}'");
			return;
		}

		if (!TryInvoke(entry, remaining))
		{
			_terminal.Log($"Invalid arguments for '{commandName}'");
		}
	}

	private bool TryInvoke(CommandEntry entry, string commandString)
	{
		var parameters = entry.Method.GetParameters();
		var args = new object[parameters.Length];

		for (var i = 0; i < parameters.Length; i++)
		{
			var paramType = parameters[i].ParameterType;

			if (paramType == typeof(Terminal))
			{
				args[i] = _terminal;
				continue;
			}

			if (!Parsers.TryGetValue(paramType, out var parser))
			{
				return false;
			}

			if (!parser.TryParse(commandString, out args[i], out var consumed))
			{
				return false;
			}

			commandString = commandString.Substring(consumed).TrimStart();
		}

		object target = null;
		if (!entry.IsStatic)
		{
			target = _registry.GetInstance(entry.MonoType);
			if (target == null)
			{
				_terminal.Log($"No active '{entry.MonoType.Name}' found in scene.");
				return true;
			}
		}

		entry.Method.Invoke(target, args);
		return true;
	}
}
}