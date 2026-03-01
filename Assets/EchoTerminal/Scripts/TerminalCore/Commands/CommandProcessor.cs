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
		{ typeof(string), new StringParser() },
		{ typeof(UnityEngine.GameObject), new GameObjectParser() }
	};

	private readonly CommandRegistry _registry;

	private readonly Terminal _terminal;

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
		var commandName = space == -1 ? remaining : remaining[..space];
		remaining = space == -1 ? string.Empty : remaining[space..].TrimStart();

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
		UnityEngine.GameObject singleTarget = null;
		if (!entry.IsStatic && Parsers[typeof(UnityEngine.GameObject)].TryParse(commandString, out var goObj, out var goConsumed))
		{
			var targetName = commandString[1..goConsumed];
			commandString = commandString[goConsumed..].TrimStart();
			singleTarget = goObj as UnityEngine.GameObject;
			if (singleTarget == null)
			{
				_terminal.Log($"No GameObject named '{targetName}' found in scene.");
				return true;
			}
		}

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

		if (entry.IsStatic)
		{
			var result = entry.Method.Invoke(null, args);
			if (result is string message)
			{
				_terminal.Log(message);
			}

			return true;
		}

		var targets = _registry.GetInstances(entry.MonoType);
		var invoked = false;

		foreach (var target in targets)
		{
			if (singleTarget != null && target.gameObject != singleTarget)
			{
				continue;
			}

			invoked = true;
			var result = entry.Method.Invoke(target, args);
			if (result is string message)
			{
				_terminal.Log(message);
			}
		}

		if (!invoked)
		{
			_terminal.Log(singleTarget != null
				? $"No '{entry.MonoType.Name}' on '{singleTarget.name}' found in scene."
				: $"No active '{entry.MonoType.Name}' found in scene.");
		}

		return true;
	}
}
}